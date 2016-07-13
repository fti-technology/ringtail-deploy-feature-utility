using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Mono.Options;
using RingtailDeployFeatureUtility.data;

namespace RingtailDeployFeatureUtility
{
   
    /// <summary>
    /// Simple command line utility utilized by various Ringtail installation tools to query and commit feature keys.
    /// Feature keys are used to filter what features are exposed in the product.  The keys are eventually retired.
    /// This allows customers to accept bug fixes only , new features, or beta features - or any combination.
    /// The Static csv file is generated a build time based on each feature.
    /// The keys must be written once to the Portal database before upgrade - keys are not runtime configurable
    /// </summary>
    class Program
    {
        ////////////////////////////////////////////////////////////
        // Return codes:
        // 0 - success
        // 1 - bad command line - error parsing
        // 2 - disply help
        // 3 - static csv file not found or could not be read
        // 4 - error writing build data key file
        // 5 - key list deserialization error
        // 6 - DB error
        // 7 - Error getting current database version
        // 8 - Error syncing builk data file from DB
        ////////////////////////////////////////////////////////////
        static int Main(string[] args)
        {

#if DEBUG
            Debugger.Launch();
#endif
            var serializer = new JavaScriptSerializer();

            // command line options
            CommandLineOptions cmdLineOpts = new CommandLineOptions();

            List<string> extra;
            var commandLineWasProcessed = ProcessCommandLineOpts(args, cmdLineOpts);

            // Show help if the option was passed or there was a problem processing command line args
            if (cmdLineOpts.show_help || !commandLineWasProcessed)
            {
                Help.ShowHelp(cmdLineOpts.optionSet, serializer);
                return 2;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Return true/false based on if the specified portal has had keys written to it already
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            if (!string.IsNullOrEmpty(cmdLineOpts.portalConnectionString) && cmdLineOpts.hasKeys)
            {
                if (cmdLineOpts.useBase64Encoding)
                {
                    cmdLineOpts.portalConnectionString = TextOperations.GetConnectionStringFromB64(cmdLineOpts.portalConnectionString);
                }

                var ret = DataBaseOperations.DatabaseKeysExist(cmdLineOpts.portalConnectionString);
                var serializedResult = serializer.Serialize(ret);
                Console.WriteLine(serializedResult);
                return 0;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Retrieve the illuminated feature keys from the database  - return json keys list
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            if (!string.IsNullOrEmpty(cmdLineOpts.portalConnectionString) && cmdLineOpts.getfeaturekeys)
            {
                if (cmdLineOpts.useBase64Encoding)
                {
                    cmdLineOpts.portalConnectionString = TextOperations.GetConnectionStringFromB64(cmdLineOpts.portalConnectionString);
                }

                List<string> featuresList;
                var ret = DataBaseOperations.GetIlluminatedFeatures(cmdLineOpts.portalConnectionString, out featuresList);
                var serializedResult = serializer.Serialize(featuresList);
                Console.WriteLine(serializedResult);
                return ret;
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Retrieve the keys from the static csv file and return them as a string in the a kvp Json form.
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            if ((!string.IsNullOrEmpty(cmdLineOpts.pathToSqlFile)) || (cmdLineOpts.useDefaultKeyFile))
            {
                string serializedResult;
                int ret = Operations.HandleStaticSqlFile(cmdLineOpts, serializer, out serializedResult);
                Console.WriteLine(serializedResult);
                return ret;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Generate bulk load data file for insertion into database based on selected feature keys
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            if ((!string.IsNullOrEmpty(cmdLineOpts.pathToBulkDataKeyFile)) && (!string.IsNullOrEmpty(cmdLineOpts.featureLaunchKeys) || !string.IsNullOrEmpty(cmdLineOpts.featureLaunchKeysPath)))
            {
                return Operations.WriteBuklLoadFeatureFile(args, cmdLineOpts, serializer);
            }
            else if ( (!string.IsNullOrEmpty(cmdLineOpts.pathToBulkDataKeyFile)) && (!string.IsNullOrEmpty(cmdLineOpts.portalConnectionString)))
            {
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                /// This case is responsible for getting the extended dark launch keys out of the portal 
                /// and writing them to the bulk data file for the case - essentially sync up the keys for the case.
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                return Operations.SyncPortalKeysToBulkDataFile(cmdLineOpts.portalConnectionString, cmdLineOpts.pathToBulkDataKeyFile);
            }
            else if (cmdLineOpts.testMode && (string.IsNullOrEmpty(cmdLineOpts.featureLaunchKeys)))
            {
                // Simple mode used for testing
                var darkLaunchKeysList = MockData.GenerateMockFeatureLaunchKeysList();
                var serializedResult = serializer.Serialize(darkLaunchKeysList);
                Console.WriteLine(serializedResult);

                return 0;
            }
            else
            {
                Help.ShowHelp(cmdLineOpts.optionSet, serializer);
                return 0;
            }
        }

        /// <summary>
        /// Handle parsing the command line options
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cmdLineOpts"></param>
        /// <returns></returns>
        private static bool ProcessCommandLineOpts(string[] args, CommandLineOptions cmdLineOpts)
        {  
            try
            {
                cmdLineOpts.extra = cmdLineOpts.optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                Console.Write("{0}: ", exeName);
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", exeName);
                return false;
            }

            if (cmdLineOpts.extra.Count > 0)
            {
                Console.Write("Error, {0} additional argument(s) found", cmdLineOpts.extra.Count);
                return false;
            }

            return true;
        }
    }
}
