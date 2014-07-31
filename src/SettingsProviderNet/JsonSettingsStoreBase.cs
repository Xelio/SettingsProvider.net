using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SettingsProviderNet
{
    public abstract class JsonSettingsStoreBase : ISettingsStorage
    {
        protected abstract void WriteTextFile(string filename, string fileContents);
        
        protected abstract string ReadTextFile(string filename);

        public void Save<T>(string key, T settings)
        {
            var filename = key + ".settings";

            string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });

            WriteTextFile(filename, jsonString);
        }

        public T Load<T>(string key) where T : new()
        {
            return LoadAndUpdate(key, new T());
        }

        public T LoadAndUpdate<T>(string key, T settings)
        {
            var filename = key + ".settings";
            var readTextFile = ReadTextFile(filename);

            if (!string.IsNullOrEmpty(readTextFile))
            {
                JsonConvert.PopulateObject(readTextFile, settings, new Newtonsoft.Json.JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                });
            }

            return settings;
        }
    }
}