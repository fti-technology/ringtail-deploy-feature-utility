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
            bool show_help = false;
            string pathToSqlFile = null;
            string sqlConnectionString = null;
            string darkLaunchKeys = null;
            bool testMode = false;
            bool useDefaultKeyFile = false;
            string filter = null;

            var p = new OptionSet() {
                { "s|sqlfile=", "path to static sql file.", v => pathToSqlFile = v},
                { "f|filter=", "filter the keys based on the following: PREVIEW, BETA, RC",v=> filter = v },
                { "g|getkeys", "attempts to retrieve keys from the default key file if present", v=> useDefaultKeyFile = v != null },
                { "c|connectionstring=", "the sql connection string too the Portal database.", v => sqlConnectionString = v },
                { "k|keys=", "the keys to commit to the database (Json list), the form should be: \"[\"Key1\",\"Key3\",\"Key6\"]\"", v => darkLaunchKeys = v },
#if DEBUG
                { "t|test", "test value with mock data", v => testMode = v != null },
#endif
                { "h|help",  "show this message and exit", v => show_help = v != null }};

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                Console.Write("{0}: ", exeName);
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", exeName);
                return 1;
            }

            if (show_help || extra.Count > 0)
            {
                ShowHelp(p);
                return 2;
            }

            // Retrieve the keys from the static csv file and return them as a string in the a kvp Json form.
            if ((!string.IsNullOrEmpty(pathToSqlFile)) || (useDefaultKeyFile))
            {
                var darkLaunchKeysList = new List<KeyValuePair<string,string>>();
                string pathToFile;
                if (useDefaultKeyFile)
                {
                    pathToFile = RingtailStaticKeyFile;
                }
                else
                {
                    pathToFile = pathToSqlFile;
                }

                if (!System.IO.File.Exists(pathToFile))
                {
                    Console.WriteLine("Error, static data file not found.");
                    return 3;
                }
                
                // Simple filtering based on type
                KeyTypesFilter filterType = KeyTypesFilter.ALL;
                if (!string.IsNullOrEmpty(filter))
                {
                    if (!Enum.TryParse(filter, true, out filterType))
                    {
                        filterType = KeyTypesFilter.ALL; // Default back to all if we go garbage
                    }
                }

                // Parse the csv and serialize the results
                darkLaunchKeysList = TextOperations.ParseCSV(pathToFile, filterType).ToList();

                var serializedResult = serializer.Serialize(darkLaunchKeysList);
                Console.WriteLine(serializedResult);
                return 0;
            }
            else if ((!string.IsNullOrEmpty(pathToSqlFile)) && (!string.IsNullOrEmpty(pathToSqlFile)) && (!string.IsNullOrEmpty(darkLaunchKeys)))
            {
                List<string> darkLaunchKeysList = serializer.Deserialize<List<string>>(darkLaunchKeys);
                if (darkLaunchKeysList.Any())
                {
                    /////////////////////////////
                    // TODO: WRITE KEYS TO DB HERE
                    /////////////////////////////
                }

            }
            else if (testMode && (string.IsNullOrEmpty(darkLaunchKeys)))
            {
                // Simple mode used for testing
                var darkLaunchKeysList = new List<string>();
                darkLaunchKeysList.Add("MockKey1");
                darkLaunchKeysList.Add("MockKey2");
                darkLaunchKeysList.Add("MockKey3");
                darkLaunchKeysList.Add("MockKey4");

                var serializedResult = serializer.Serialize(darkLaunchKeysList);
                Console.WriteLine(serializedResult);

                return 0;
            }
            else
            {
                ShowHelp(p);
                return 0;
            }

            return 0;
        }

        /// <summary>
        /// Help message
        /// </summary>
        /// <param name="p"></param>
        static void ShowHelp(OptionSet p)
        {
            var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.WriteLine("Usage: {0} [OPTIONS]+ message", exeName);
            Console.WriteLine("Queries for the available Ringtail feature strings and also commits the selected strings into the database.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

    }
}
