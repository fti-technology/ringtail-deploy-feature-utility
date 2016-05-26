using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RingtailDeployFeatureUtility
{
    class Operations
    {
        public static string RingtailStaticKeyFile = "RingtailDarkKeys.csv";
        public static string RingtailBulkDataFile = "generated_feature_keys.txt";

        /// <summary>
        /// Parse the static csv file and send back the json keys with the minor keys
        /// </summary>
        /// <param name="cmdLineOpts"></param>
        /// <param name="serializer">JavaScriptSerializer</param>
        /// <param name="serializedResult">serialized results</param>
        /// <returns></returns>
        internal static int HandleStaticSqlFile(CommandLineOptions cmdLineOpts, JavaScriptSerializer serializer, out string serializedResult)
        {
            serializedResult = String.Empty;

            string pathToFile;
            if (cmdLineOpts.useDefaultKeyFile)
            {
                pathToFile = RingtailStaticKeyFile;
            }
            else
            {
                pathToFile = cmdLineOpts.pathToSqlFile;
            }

            if (!File.Exists(pathToFile))
            {
                Console.WriteLine("Error, static data file not found.");
                {
                    return 3;
                }
            }

            // Simple filtering based on type
            KeyTypesFilter filterType = KeyTypesFilter.ALL;
            if (!String.IsNullOrEmpty(cmdLineOpts.filter))
            {
                if (!Enum.TryParse(cmdLineOpts.filter, true, out filterType))
                {
                    filterType = KeyTypesFilter.ALL; // Default back to all if we go garbage
                }
            }

            // Parse the csv and serialize the results
            var darkLaunchKeysList = TextOperations.ParseCSV(pathToFile, filterType);
            serializedResult = serializer.Serialize(darkLaunchKeysList);
            return 0;
        }

        internal static int WriteBuklLoadFeatureFile(string[] args, CommandLineOptions cmdLineOpts, JavaScriptSerializer serializer)
        {
            try
            {
                if (!string.IsNullOrEmpty(cmdLineOpts.featureLaunchKeysPath))
                {
                    if (System.IO.File.Exists(cmdLineOpts.featureLaunchKeysPath))
                    {
                        try
                        {
                            cmdLineOpts.featureLaunchKeys = File.ReadAllText(cmdLineOpts.featureLaunchKeysPath);
                        }
                        catch (Exception e)
                        {
                            Console.Write("Error, key list file could not be read: ", e.Message);
                            cmdLineOpts.featureLaunchKeys = null;
                        }
                    }
                    else
                    {
                        Console.Write("Error, key list file not found at: ", cmdLineOpts.featureLaunchKeysPath);
                        cmdLineOpts.featureLaunchKeys = null;
                    }
                    
                }
                else if (cmdLineOpts.useBase64Encoding)
                {
                    byte[] byteArray = Convert.FromBase64String(cmdLineOpts.featureLaunchKeys);
                    cmdLineOpts.featureLaunchKeys = Encoding.UTF8.GetString(byteArray);
                }
                else
                {
                    var formatedString = FormatKeysInputJson(args, cmdLineOpts.GetKeysCommandLine());
                    if (!string.IsNullOrEmpty(formatedString))
                    {
                        cmdLineOpts.featureLaunchKeys = formatedString;
                    }
                }

                List<KeyDataObject> darkLaunchKeysList = serializer.Deserialize<List<KeyDataObject>>(cmdLineOpts.featureLaunchKeys);
                if (darkLaunchKeysList.Any())
                {
                    if (!TextOperations.WriteBuklLoadFeatureFile(cmdLineOpts.pathToBulkDataKeyFile, darkLaunchKeysList,RingtailBulkDataFile))
                    {
                        return 4;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Error, key list could not be deserialized: ", e.Message);
                {
                    return 5;
                }
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
                        var startIndex = xy.IndexOf(fullKeysCommandLine, StringComparison.Ordinal);
                        var endIndex = xy.IndexOf("]", startIndex, StringComparison.Ordinal);
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
    }
}
