using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SettingsProviderNet
{
    public class FileStorage : JsonSettingsStoreBase
    {
        private readonly string fileName;
        public FileStorage(string fileName)
        {
            this.fileName = fileName;
        }
        protected override void WriteTextFile(string dummy, string fileContents)
        {
            File.WriteAllText(this.fileName, fileContents);
        }

        protected override string ReadTextFile(string dummy)
        {
            return File.Exists(this.fileName) ? File.ReadAllText(this.fileName) : null;
        }
    }
}
