
using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

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
        public static IEnumerable<KeyValuePair<string,string>> ParseCSV(string path, KeyTypesFilter keyFilter = KeyTypesFilter.ALL)
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
                    KeyValuePair<string, string> keyAndDesc;
                    if (fields.Length == 3)
                    {
                        keyAndDesc = new KeyValuePair<string, string>(fields[0], fields[1]);
                        
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
                            yield return keyAndDesc;
                        }
                    }
                }
            }
        }
    }
}