using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SettingsProviderNet
{
    public class PortableStorage : JsonSettingsStoreBase
    {
        public PortableStorage()
        {
        }

        protected override void WriteTextFile(string filename, string fileContents)
        {
            var settingsFolder = GetProgramFolder();
            if (!Directory.Exists(settingsFolder))
                Directory.CreateDirectory(settingsFolder);
            File.WriteAllText(Path.Combine(settingsFolder, filename), fileContents);
        }

        protected override string ReadTextFile(string filename)
        {
            var settingsFilename = Path.Combine(GetProgramFolder(), filename);
            return File.Exists(settingsFilename) ? File.ReadAllText(settingsFilename) : null;
        }

        private string GetProgramFolder()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
