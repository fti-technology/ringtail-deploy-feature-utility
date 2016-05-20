using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingtailDeployFeatureUtility
{
    class DataBaseOperations
    {

        public static int GetIlluminatedFeatures(string connectionString, out List<string> featureList)
        {
            featureList = new List<string>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {

                    con.Open();

                    using (SqlCommand command =new SqlCommand("SELECT feature_key FROM dbo.featureset_list", con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                featureList.Add(reader.GetString(reader.GetOrdinal("feature_key")));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error, connecting to database: {0}", e.Message);
                return 6;
            }

            return 0;
        }

        // "Data Source=ServerName;" + "Initial Catalog=DataBaseName;" +"User id=UserName;" + "Password=Secret;";
        public static bool DatabaseKeysExist(string connectionString)
        {
            string featureSetListVersion = null;
            string ringtailApplicationVersion = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {

                    con.Open();

                    using (
                        SqlCommand command =
                            new SqlCommand(
                                "SELECT theValue FROM dbo.list_variables WHERE theLabel = 'FeatureSetListVersion'", con)
                        )
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                featureSetListVersion = reader.GetString(reader.GetOrdinal("theValue"));
                            }
                        }
                    }

                    using (
                        SqlCommand command =
                            new SqlCommand(
                                "SELECT theValue FROM dbo.list_variables WHERE theLabel = 'RingtailApplicationVersion'",
                                con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ringtailApplicationVersion = reader.GetString(reader.GetOrdinal("theValue"));
                            }
                        }
                    }
                    con.Close();
                }

            }
            catch (Exception){}

            try
            {

            
                if (!string.IsNullOrEmpty(featureSetListVersion) && !string.IsNullOrEmpty(ringtailApplicationVersion))
                {
                    //8.6.015.1892
                    Version appVersion;
                    Version featureWrittenVersion;

                    if (Version.TryParse(ringtailApplicationVersion, out appVersion) &&
                        Version.TryParse(featureSetListVersion, out featureWrittenVersion))
                    {
                        return featureWrittenVersion.CompareTo(appVersion) >= 0;
                    }
                
                    if (string.Compare(ringtailApplicationVersion, featureSetListVersion,StringComparison.InvariantCultureIgnoreCase) == 0)
                            return true;
                }
            }
            catch (Exception){}

            return false;
        }
    }
}
