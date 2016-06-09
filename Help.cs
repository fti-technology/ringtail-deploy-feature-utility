using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Mono.Options;
using RingtailDeployFeatureUtility.data;

namespace RingtailDeployFeatureUtility
{
    class Help
    {
        /// <summary>
        /// Displays Help to the command line
        /// </summary>
        /// <param name="p">Command line options</param>
        /// <param name="serializer">JavaScriptSerializer</param>
        internal static void ShowHelp(OptionSet p, JavaScriptSerializer serializer)
        {
            var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.WriteLine("Usage: {0} [OPTIONS]+ message", exeName);
            Console.WriteLine("Queries for the available Ringtail feature strings and also generates a bulk data file for database import.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();

            Console.WriteLine("Typical usage");
            Console.WriteLine("Get the keys in JSON form from the default file, assuming {0} is next to {1}", Operations.RingtailStaticKeyFile, exeName);
            Console.WriteLine("cmd>{0}.exe --getkeys", exeName);
            Console.WriteLine();

            Console.WriteLine("Get the keys in JSON form from a specific file, with a filter");
            Console.WriteLine("cmd>{0}.exe --sqlfile=\"d:\\somepath\\myKeyfile.csv\" --filter=\"DEVELOPMENT\"", exeName);
            Console.WriteLine();

            // var keyExmple = "\"[{\"FeatureKey\":\"KEY3\",\"Description\":\"DESCRIPTION of Key3 - ready for general release\",\"MinorKey\":\"8.6.1002\"}]\"";
            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys using a base64 endcoded string (preferred)");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keys=\"W3sgIkZlYXR1cmVLZXkiOiJLRVkxIiwiTWlub3JLZXkiOiIifV0=\" /base64", exeName);
            Console.WriteLine();

            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keys=\"{1}\"", exeName, serializer.Serialize(MockData.GenerateMockFeatureLaunchKeysList()));
            Console.WriteLine();

            Console.WriteLine("Query the specified portal database to determine if the keys have been commited, returns a true/false response.");
            Console.WriteLine("Database connection string should be in this format: \"Data Source = (local);Initial Catalog = PortalBaseName;User id = UserName;Password = Secret;\"");
            Console.WriteLine("cmd>{0}.exe /haskeys --portalconnection=\"Data Source =192.168.1.2;Initial Catalog = MyPortal;User id = MyPortalUser;Password = abc123;\"", exeName);
            Console.WriteLine("(OR)");
            Console.WriteLine("cmd>{0}.exe /haskeys --portalconnection=\"RGF0YSBTb3VyY2UgPTE5Mi4xNjguMS4yO0luaXRpYWwgQ2F0YWxvZyA9IE15UG9ydGFsO1VzZXIgaWQgPSBNeVBvcnRhbFVzZXI7UGFzc3dvcmQgPSBhYmMxMjM7\" /base64", exeName);

            Console.WriteLine("Query the specified portal database to retrieve the current illuminated feature keys.");
            Console.WriteLine("Database connection string should be in this format: \"Data Source = (local);Initial Catalog = PortalBaseName;User id = UserName;Password = Secret;\"");
            Console.WriteLine("cmd>{0}.exe /getfeaturekeys --portalconnection=\"Data Source =192.168.1.2;Initial Catalog = MyPortal;User id = MyPortalUser;Password = abc123;\"", exeName);
            Console.WriteLine("(OR)");
            Console.WriteLine("cmd>{0}.exe /getfeaturekeys --portalconnection=\"RGF0YSBTb3VyY2UgPTE5Mi4xNjguMS4yO0luaXRpYWwgQ2F0YWxvZyA9IE15UG9ydGFsO1VzZXIgaWQgPSBNeVBvcnRhbFVzZXI7UGFzc3dvcmQgPSBhYmMxMjM7\" /base64", exeName);
            Console.WriteLine("(OR)");
            Console.WriteLine("Create the bulk data file used for import into the database on a set of keys using file that contains key data.");
            Console.WriteLine("cmd>{0}.exe --bulkdatapath==\"d:\\somepath\\somesubdir\" --keysfile=\"{1}\"", exeName, @"D:\myKeysFile.json");
            Console.WriteLine();

        }
    }
}
