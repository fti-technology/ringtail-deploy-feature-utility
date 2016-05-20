using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingtailDeployFeatureUtility.data
{
    class MockData
    {
        internal static List<KeyDataObject> GenerateMockDarkLaunchKeysList()
        {
            var darkLaunchKeysList = new List<KeyDataObject>();
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "some description",
                FeatureKey = "MockFeatureKey1",
                MinorKey = ""
            });
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "",
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100"
            });
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "this should turn on blah",
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001"
            });
            darkLaunchKeysList.Add(new KeyDataObject
            {
                Description = "description of this",
                FeatureKey = "MockFeatureKey4",
                MinorKey = "8.6.001"
            });
            return darkLaunchKeysList;
        }

        internal static List<KeyDataObjectBase> GenerateMockDarkLaunchKeysListInput()
        {
            var darkLaunchKeysList = new List<KeyDataObjectBase>();
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {

                FeatureKey = "MockFeatureKey1",
                MinorKey = ""
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey2",
                MinorKey = "8.5.100"
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey3",
                MinorKey = "8.6.001"
            });
            darkLaunchKeysList.Add(new KeyDataObjectBase
            {
                FeatureKey = "MockFeatureKey4",
                MinorKey = "8.6.001"
            });
            return darkLaunchKeysList;
        }
    }
}
