using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    if (cmdLineOpts.useBase64Encoding)
                    {
                        byte[] byteArray = Convert.FromBase64String(cmdLineOpts.darkLaunchKeys);
                        cmdLineOpts.darkLaunchKeys = Encoding.UTF8.GetString(byteArray);
                    }
                    else
                    {
                        var formatedString = FormatKeysInputJson(args, cmdLineOpts.GetKeysCommandLine());
                        if (!string.IsNullOrEmpty(formatedString))
                        {
                            cmdLineOpts.darkLaunchKeys = formatedString;
                        }
                    }

                    //Debugger.Launch();
                    List<KeyDataObjectBase> darkLaunchKeysList = serializer.Deserialize<List<KeyDataObjectBase>>(cmdLineOpts.darkLaunchKeys);
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

        /// <summary>
        /// Function responsible for cleaning up the json passed in on the command line 
        /// becuase windows will remove and add quotes on the standard command line - thus go
        /// to the Environment.CommandLine to get the properly formated string ( i.e. the exact string that is passed in )
        /// Hack the JSon string off the Environment.CommandLine and return just that portion.
        /// </summary>
        /// <param name="args">command line args</param>
        /// <param name="keysCommandLineToken">expects the command line token for the json keys data - i.e. -keys=</param>
        /// <returns></returns>
        private static string FormatKeysInputJson(string[] args, string keysCommandLineToken)
        {
            
            if (args.Length > 0)
            {

                var keysIndex = Array.FindIndex(args, t => (t.StartsWith(keysCommandLineToken)));
                if (keysIndex != -1)
                {
                    try
                    {
                        var fullKeysCommandLine = keysCommandLineToken + "\"";
                        var xy = Environment.CommandLine;
                        var startIndex = xy.IndexOf(fullKeysCommandLine);
                        var endIndex = xy.IndexOf("]", startIndex);
                        var keysData = xy.Substring(startIndex + fullKeysCommandLine.Length, endIndex - startIndex - keysCommandLineToken.Length);
                        if (!string.IsNullOrEmpty(keysData))
                            return keysData;                        
                    }
                    catch (Exception e)
                    {
                        Console.Write("Error, {0} command line option could not be parsed into the proper JSON form: {1}", keysCommandLineToken, e.Message);
                    }
                }
            }

            return null;
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
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "",
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100"
            });
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

        private static List<KeyDataObjectBase> GenerateMockDarkLaunchKeysListInput()
        {
            var darkLaunchKeysList = new List<KeyDataObjectBase>();
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {

                FeatureKey = "MockFeatureKey1",
                MinorKey = ""
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100"
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001"
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
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
            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys using a base64 endcoded string (preferred)");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keys=\"W3sgIkZlYXR1cmVLZXkiOiJLRVkxIiwiTWlub3JLZXkiOiIifV0=\" /base64", exeName);
            Console.WriteLine();


            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keys=\"{1}\"", exeName, serializer.Serialize(GenerateMockDarkLaunchKeysListInput()));
            Console.WriteLine();
        }

    }
}
