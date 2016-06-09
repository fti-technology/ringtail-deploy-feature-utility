using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingtailDeployFeatureUtility.data
{
    class MockData
    {
        internal static List<KeyDataObject> GenerateMockFeatureLaunchKeysList()
        {
            var featureLaunchKeysList = new List<KeyDataObject>();
            featureLaunchKeysList.Add(new KeyDataObject
            {
                Description = "some description",
                FeatureKey = "MockFeatureKey1",
                MinorKey = ""
            });
            featureLaunchKeysList.Add(new KeyDataObject
            {
                Description = "",
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100"
            });
            featureLaunchKeysList.Add(new KeyDataObject
            {
                Description = "this should turn on blah",
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001"
            });
            featureLaunchKeysList.Add(new KeyDataObject
            {
                Description = "description of this",
                FeatureKey = "MockFeatureKey4",
                MinorKey = "8.6.001"
            });
            return featureLaunchKeysList;
        }

        internal static List<KeyDataObjectBase> GenerateMockFeatureLaunchKeysListInput()
        {
            var featureLaunchKeysList = new List<KeyDataObjectBase>();
            featureLaunchKeysList.Add(new KeyDataObjectBase
            {

                FeatureKey = "MockFeatureKey1",
                MinorKey = "",
                KeyType = KeyTypesFilter.Development.ToString()
            });
            featureLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100",
                KeyType = KeyTypesFilter.GA.ToString()
            });
            featureLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001",
                KeyType = KeyTypesFilter.GA.ToString()
            });
            featureLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey4",
                MinorKey = "8.6.001",
                KeyType = KeyTypesFilter.GA.ToString()
            });
            return featureLaunchKeysList;
        }
    }
}
