using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Mono.Options;

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
        static string RingtailStaticKeyFile = "RingtailDarkKeys.csv";
        static string RingtailBulkDataFile = "generated_feature_keys.txt";

        ////////////////////////////////////////////////////////////
        // Return codes:
        // 0 - success
        // 1 - bad command line - error parsing
        // 2 - disply help
        // 3 - static csv file not found or could not be read
        // 4 - error writing build data key file
        // 5 - key list deserialization error
        ////////////////////////////////////////////////////////////
        static int Main(string[] args)
        {
            var serializer = new JavaScriptSerializer();

            /////////////////////////////////////////////////////////////////////////////
            // MODE 1 QUERY FOR OPTIONS
            // Need Path to SQL or useDefaultKeyFile (looks next to exe for file)

            // Mode 2 INSERT 
            // Need Path to SQL
            // Connection String
            // KEYS
            /////////////////////////////////////////////////////////////////////////////


            // command line options
            CommandLineOptions cmdLineOpts = new CommandLineOptions();

            List<string> extra;
            var commandLineWasProcessed = ProcessCommandLineOpts(args, cmdLineOpts);

            if (cmdLineOpts.show_help || !commandLineWasProcessed)
            {
                ShowHelp(cmdLineOpts.optionSet, serializer);
                return 2;
            }

            // Retrieve the keys from the static csv file and return them as a string in the a kvp Json form.
            if ((!string.IsNullOrEmpty(cmdLineOpts.pathToSqlFile)) || (cmdLineOpts.useDefaultKeyFile))
            {
                string pathToFile;
                if (cmdLineOpts.useDefaultKeyFile)
                {
                    pathToFile = RingtailStaticKeyFile;
                }
                else
                {
                    pathToFile = cmdLineOpts.pathToSqlFile;
                }

                if (!System.IO.File.Exists(pathToFile))
                {
                    Console.WriteLine("Error, static data file not found.");
                    return 3;
                }
                
                // Simple filtering based on type
                KeyTypesFilter filterType = KeyTypesFilter.ALL;
                if (!string.IsNullOrEmpty(cmdLineOpts.filter))
                {
                    if (!Enum.TryParse(cmdLineOpts.filter, true, out filterType))
                    {
                        filterType = KeyTypesFilter.ALL; // Default back to all if we go garbage
                    }
                }

                // Parse the csv and serialize the results
                var darkLaunchKeysList = TextOperations.ParseCSV(pathToFile, filterType);
                var serializedResult = serializer.Serialize(darkLaunchKeysList);
                Console.WriteLine(serializedResult);
                return 0;
            }
            else if ((!string.IsNullOrEmpty(cmdLineOpts.pathToBulkDataKeyFile)) && (!string.IsNullOrEmpty(cmdLineOpts.darkLaunchKeys)))
            {
                try
                {
                    List<string> darkLaunchKeysList = serializer.Deserialize<List<string>>(cmdLineOpts.darkLaunchKeys);
                    if (darkLaunchKeysList.Any())
                    {
                        if (!TextOperations.WriteBuklLoadFeatureFile(cmdLineOpts.pathToBulkDataKeyFile, darkLaunchKeysList, RingtailBulkDataFile))
                            return 4;
                    }
                }
                catch (Exception e)
                {
                    Console.Write("Error, key list could not be deserialized: ", e.Message);
                    return 5;
                }
                

            }
            else if (cmdLineOpts.testMode && (string.IsNullOrEmpty(cmdLineOpts.darkLaunchKeys)))
            {
                // Simple mode used for testing
                var darkLaunchKeysList = GenerateMockDarkLaunchKeysList();

                var serializedResult = serializer.Serialize(darkLaunchKeysList);
                Console.WriteLine(serializedResult);

                return 0;
            }
            else
            {
                ShowHelp(cmdLineOpts.optionSet, serializer);
                return 0;
            }

            return 0;
        }

        private static List<KeyDataObject> GenerateMockDarkLaunchKeysList()
        {
            var darkLaunchKeysList = new List<KeyDataObject>();
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "some description",
                FeatureKey = "MockFeatureKey1",
                MinorKey = ""
            });
            darkLaunchKeysList.Add(new KeyDataObject {Description = "", FeatureKey = "MockFeatureKey2", MinorKey = "8.5.100"});
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "this should turn on blah",
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001"
            });
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "description of this",
                FeatureKey = "MockFeatureKey4",
                MinorKey = "8.6.001"
            });
            return darkLaunchKeysList;
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


        /// <summary>
        /// Help message
        /// </summary>
        /// <param name="p"></param>
        static void ShowHelp(OptionSet p, JavaScriptSerializer serializer)
        {
            var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.WriteLine("Usage: {0} [OPTIONS]+ message", exeName);
            Console.WriteLine("Queries for the available Ringtail feature strings and also generates a bulk data file for database import.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();

            Console.WriteLine("Typical usage");
            Console.WriteLine("Get the keys in JSON form from the default file, assuming {0} is next to {1}", RingtailStaticKeyFile, exeName);
            Console.WriteLine("cmd>{0}.exe --getkeys", exeName);
            Console.WriteLine();

            Console.WriteLine("Get the keys in JSON form from a specific file, with a filter");
            Console.WriteLine("cmd>{0}.exe sqlfile=\"d:\\somepath\\myKeyfile.csv\" --filter=\"PREVIEW\"", exeName);
            Console.WriteLine();

           // var keyExmple = "\"[{\"FeatureKey\":\"KEY3\",\"Description\":\"DESCRIPTION of Key3 - ready for general release\",\"MinorKey\":\"8.6.1002\"}]\"";
            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keys=\"{1}\"", exeName, serializer.Serialize(GenerateMockDarkLaunchKeysList()));
            Console.WriteLine();
        }

    }
}
