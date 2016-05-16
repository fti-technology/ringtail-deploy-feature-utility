using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingtailDeployFeatureUtility
{
    class KeyDataObjectBase
    {
        public string FeatureKey { get; set; }
        public string MinorKey { get; set; }
    }

    class KeyDataObject : KeyDataObjectBase
    {
        public string Description { get; set; }   
    }
}
