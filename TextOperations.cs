
using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.IO;

namespace RingtailDeployFeatureUtility
{
    /// <summary>
    /// Filter for type of Keys to return
    /// </summary>
    [Flags]
    public enum KeyTypesFilter
    {
        PREVIEW = 1,
        BETA = 2,
        RC = 4,
        ALL = PREVIEW | BETA | RC
    }


    /// <summary>
    /// Simple helper for text and CSV operations
    /// </summary>
    class TextOperations
    {
        // if preview keys are allowed, allow all types
        private static KeyTypesFilter PREVIEW_KEYS = KeyTypesFilter.RC | KeyTypesFilter.BETA | KeyTypesFilter.PREVIEW;
        // if BETA KEYS are allowed, allow RC and BETA types
        private static KeyTypesFilter BETA_KEYS = KeyTypesFilter.RC | KeyTypesFilter.BETA;
        // if RC keys are allowed, allow only RC
        private static KeyTypesFilter RC_KEYS = KeyTypesFilter.RC;

        /// <summary>
        /// Parse the static CSV keys file and return a list of keys and descriptions based on the filter and rules
        /// </summary>
        /// <param name="path"></param>
        /// <param name="keyFilter"></param>
        /// <returns></returns>
        public static IEnumerable<KeyDataObject> ParseCSV(string path, KeyTypesFilter keyFilter = KeyTypesFilter.ALL)
        {
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { ";", "," });
                parser.HasFieldsEnclosedInQuotes = true;

                // Skip over header line.
                //parser.ReadLine();
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    KeyDataObject data;
                    if (fields.Length == 4)
                    {
                        data = new KeyDataObject()
                        {
                            Description = fields[1],
                            FeatureKey = fields[0],
                            MinorKey = fields[3]
                        };
                        
                        bool includeKeyRow = false;
                        if (keyFilter == KeyTypesFilter.ALL)
                        {
                            includeKeyRow = true;
                        }
                        else
                        {

                            KeyTypesFilter rowKeyType;
                            if (Enum.TryParse(fields[2], true, out rowKeyType))
                            {
                                bool includeInPreview = PREVIEW_KEYS.HasFlag(rowKeyType);
                                bool includeInBeta = BETA_KEYS.HasFlag(rowKeyType);
                                bool includeInRc = RC_KEYS.HasFlag(rowKeyType);

                                if (keyFilter.HasFlag(KeyTypesFilter.RC))
                                {
                                    includeKeyRow = includeInRc;
                                }
                                else if (keyFilter.HasFlag(KeyTypesFilter.BETA))
                                {
                                    includeKeyRow = includeInBeta;
                                }
                                else if (keyFilter.HasFlag(KeyTypesFilter.PREVIEW))
                                {
                                    includeKeyRow = includeInPreview;
                                }
                            }
                        }


                        if (includeKeyRow)
                        {
                            yield return data;
                        }
                    }
                }
            }
        }


        public static bool WriteBuklLoadFeatureFile(string bulkLoadFilePath, List<KeyDataObjectBase> featureKeyList, string defaultFileName=null)
        {
            
            if (Directory.Exists(bulkLoadFilePath))
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(bulkLoadFilePath);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        // Its a directory path
                        bulkLoadFilePath = Path.Combine(bulkLoadFilePath, defaultFileName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error, file path invalid {0} - ", bulkLoadFilePath, e.Message);
                    return false;
                }
            }

         
            var dateTimeStamp = DateTime.UtcNow.ToString();
            try
            {
                using (var streamWriter = new StreamWriter(bulkLoadFilePath, false))
                {
                    foreach (var featureKey in featureKeyList)
                    {
                        var _minorKey = "null";
                        if (!string.IsNullOrEmpty(featureKey.MinorKey))
                        {
                            _minorKey = "\"" + featureKey.MinorKey + "\"";
                        }
                        
                        streamWriter.WriteLine(string.Format("1, \"{0}\", \"{1}\", {2}", featureKey.FeatureKey, _minorKey, dateTimeStamp));
                        streamWriter.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing bulk data file, {0}", e.Message);
                return false;
            }

            return true;
        }

    }
}