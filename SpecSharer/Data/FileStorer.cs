using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.Storage
{
    internal class FileStorer : IFileStorer
    {
        public FileStorer() { }

        public void StoreBindings(string path, BindingsFileData data, bool overwrite)
        {
            string toStore = "";
            if (File.Exists(path))
            {
                toStore = Environment.NewLine + Environment.NewLine;
            }
            toStore += data.ConvertToString();
            File.AppendAllText(path, toStore);
            return;
        }
    }
}
