using System;
using System.Collections.Generic;
using System.Data;
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

        public static int GetFeaturesForPortalRev(string connectionString, out List<KeyDataObject> featureList)
        {
            featureList = new List<KeyDataObject>();
            string verStrValue = null;
            string versionValue;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {

                    con.Open();
                    using (SqlCommand command = new SqlCommand("SELECT src.theValue FROM dbo.list_variables src WHERE src.theLabel = 'RingtailDatabaseModel'", con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                verStrValue = reader.GetString(reader.GetOrdinal("theValue"));
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(verStrValue))
                    {
                        return 7;
                    }

                    using (SqlCommand comm = new SqlCommand("dbo.rf_convert_version_to_bigint", con))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        SqlParameter p1 = new SqlParameter("@version", SqlDbType.VarChar);
                        p1.Direction = ParameterDirection.Input;
                        SqlParameter p2 = new SqlParameter("@Result", SqlDbType.BigInt);
                        p2.Direction = ParameterDirection.ReturnValue;

                        p1.Value = verStrValue;
                        comm.Parameters.Add(p1);
                        comm.Parameters.Add(p2);


                        comm.ExecuteNonQuery();

                        if (p2.Value == DBNull.Value)
                        {
                            return 7;
                        }

                        versionValue = p2.Value.ToString();
                    }

                    using (SqlCommand command = new SqlCommand("SELECT * FROM dbo.featureset_list where active_version='" + versionValue + "'", con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                KeyDataObject keyDataObject = new KeyDataObject();
                                string feature_key, minor_key, date_added, description;

                                featureList.Add(
                                    keyDataObject = new KeyDataObject()
                                    {
                                        Description = reader.GetString(reader.GetOrdinal("description")),
                                        FeatureKey = reader.GetString(reader.GetOrdinal("feature_key")),
                                        MinorKey = minor_key = reader.GetString(reader.GetOrdinal("minor_key"))
                                    });
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
