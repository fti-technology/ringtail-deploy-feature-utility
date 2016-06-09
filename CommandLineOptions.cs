using System.Collections.Generic;
using Microsoft.VisualBasic;
using Mono.Options;

class CommandLineOptions
{
    // <summary>
    /// Helper class to store the command line options and their values
    /// </summary>
    private OptionSet optionSetValue;

    // used for external parsing
    const string keysCommandLine = "keys=";

    public CommandLineOptions()
    {
        

        show_help = false; 
        pathToSqlFile = null;
        pathToBulkDataKeyFile = null;
        featureLaunchKeys = null;
        testMode = false;
        useDefaultKeyFile = false;
        filter = null;
        useBase64Encoding = false;
        portalConnectionString = null;
        hasKeys = false;
        getfeaturekeys = false;
        featureLaunchKeysPath = null;
        optionSetValue = new OptionSet()
            {
                    {"s|sqlfile=", "path to static sql file.", v => pathToSqlFile = v},
                    {"f|filter=", "filter the keys based on the following: DEVELOPMENT, BETA, RC",v => filter = v},
                    {"gk|getkeys", "attempts to retrieve keys from the default key file if present",v => useDefaultKeyFile = v != null},
                    {"gf|getfeaturekeys", "attempts to retrieve illuminated feature keys in the database - (needs portalconnection string)",v => getfeaturekeys = v != null},
                    {"hk|haskeys", "determines if the feature keys have been written for the current application version - (needs portalconnection string)",v => hasKeys = v != null},
                    {"b|bulkdatapath=", "path to generate the build data key file used for importing the key data into the database",v => pathToBulkDataKeyFile = v},
                    {keysCommandLine,"the keys to commit to the database (Json list), see usage sample for JSon input form",v => featureLaunchKeys = v},
                    {"keysfile=","(place keys in file and specificy file path) the keys to commit to the database (Json list), see usage sample for JSon input form",v => featureLaunchKeysPath = v},
                    { "base64", "set this option to indicate that the keys input data or portalconnection is base64 encoded", v => useBase64Encoding = v!=null},
                    { "p|portalconnection=", "pass the full connection string to the portal db to query on", v => portalConnectionString = v },
#if DEBUG
                    { "t|test", "test value with mock data", v => testMode = v != null },
#endif
                    {"h|help", "show this message and exit", v => show_help = v != null}
            };
    }

    public string GetKeysCommandLine()
    {
        return "-" + keysCommandLine;
    }

    public bool show_help { get; set; }
    public bool useBase64Encoding { get; set; }
    public string pathToSqlFile { get; set; }
    public string portalConnectionString { get; set; }
    public string pathToBulkDataKeyFile { get; set; }
    public string featureLaunchKeys { get; set; }
    public string featureLaunchKeysPath { get; set; }
    public bool testMode { get; set; }
    public bool useDefaultKeyFile { get; set; }
    public string filter { get; set; }
    public bool hasKeys;
    public bool getfeaturekeys;
    public OptionSet optionSet { get { return optionSetValue; } }
    public List<string> extra { get; set; }

}