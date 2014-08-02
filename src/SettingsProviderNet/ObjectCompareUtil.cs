using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SettingsProviderNet
{
    public class ObjectCompareUtil
    {
        public static bool IsEqual(object a, object b)
        {
            string jsonA = JsonConvert.SerializeObject(a);
            string jsonB = JsonConvert.SerializeObject(b);

            return jsonA == jsonB;
        }
    }
}
