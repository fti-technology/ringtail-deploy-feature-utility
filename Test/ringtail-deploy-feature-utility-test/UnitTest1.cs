using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RingtailDeployFeatureUtility;
using Microsoft.VisualBasic.FileIO;

namespace ringtail_deploy_feature_utility_test
{
    [TestClass]
    public class UnitTest1
    {
        const string SampleCSVFile = "sample_static_data.csv";


        [TestMethod]
        public void TextOperations_ParseCSV_BadPath()
        {
            // Shouldn't throw an exception, but should return an empty list
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(@"Z:\HelloWorld.MISSING.CSV", KeyTypesFilter.ALL);
            var b = ret.Any();
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void TextOperations_ParseCSV_Validate_AllKeys()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(SampleCSVFile, KeyTypesFilter.ALL);
            var cnt = ret.Count();
            Assert.AreEqual(10, ret.Count());
        }

        [TestMethod]
        public void TextOperations_ParseCSV_Validate_GAKeys()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(SampleCSVFile, KeyTypesFilter.GA); 
            var cnt = ret.Count();
            Assert.AreEqual(4, ret.Count());
        }

        [TestMethod]
        public void TextOperations_ParseCSV_Validate_BetaKeys()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(SampleCSVFile, KeyTypesFilter.Beta);
            var cnt = ret.Count();
            Assert.AreEqual(7, ret.Count());
        }

        [TestMethod]
        public void TextOperations_ParseCSV_Validate_AlphaKeys()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(SampleCSVFile, KeyTypesFilter.Alpha);
            var cnt = ret.Count();
            Assert.AreEqual(9, ret.Count());
        }


        [TestMethod]
        public void TextOperations_ParseCSV_Validate_PreAlphaKeys()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.ParseCSV(SampleCSVFile, KeyTypesFilter.PreAlpha);
            var cnt = ret.Count();
            Assert.AreEqual(10, ret.Count());
        }

        [TestMethod]
        public void TextOperations_WriteBuklLoadFeatureFile_Validate_Empty()
        {
            var ret = RingtailDeployFeatureUtility.TextOperations.WriteBuklLoadFeatureFile("TextOperations_WriteBuklLoadFeatureFile_Validate_Empty.txt", new List<KeyDataObject>());
            Assert.IsTrue(ret);

            var fileGenerated = System.IO.File.Exists("TextOperations_WriteBuklLoadFeatureFile_Validate_Empty.txt");
            Assert.IsTrue(fileGenerated, "Bulk data file was not generated");
        }

        [TestMethod]
        public void TextOperations_WriteBuklLoadFeatureFile_Validate_Content()
        {
            var listOfKeys = new List<KeyDataObject>()
            {
                new KeyDataObject()
                {
                    Description = "Testing 123",
                    FeatureKey = "Feature1",
                    MinorKey = null
                },
                new KeyDataObject()
                {
                    Description = "Testing 321",
                    FeatureKey = "Feature2",
                    MinorKey = "9.9.009"
                }
            };

            var ret = RingtailDeployFeatureUtility.TextOperations.WriteBuklLoadFeatureFile("TextOperations_WriteBuklLoadFeatureFile_Validate_Content.txt", listOfKeys);
            Assert.IsTrue(ret);

            var fileGenerated = System.IO.File.Exists("TextOperations_WriteBuklLoadFeatureFile_Validate_Content.txt");
            Assert.IsTrue(fileGenerated, "Bulk data file was not generated");

            using (TextFieldParser parser = new TextFieldParser("TextOperations_WriteBuklLoadFeatureFile_Validate_Content.txt"))
            {
                parser.Delimiters = new string[] { "," };
                for (var i = 0; i < 2; i++)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                    {
                        Assert.Fail("Missing file content");
                    }
                    Assert.AreEqual(5, parts.Length);
                }
            }
        }
    }
}
