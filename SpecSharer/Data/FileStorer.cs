using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpecSharer.Storage
{
    internal class FileStorer : IFileStorer
    {
        public FileStorer() { }

        public void StoreBindings(string path, BindingsFileData data, bool overwrite, IMethodReader reader)
        {
            string toStore = "";
            if (File.Exists(path))
            {
                data = UpdateBindingsDataWithExistingContent(path, data, reader);
            }
            toStore += ProduceStringForStorage(data, path);
            File.WriteAllText(path, toStore);
            return;
        }

        internal BindingsFileData UpdateBindingsDataWithExistingContent(string path, BindingsFileData data, IMethodReader reader)
        {
            reader.SetFilePath(path);
            BindingsFileData localData = reader.ProcessBindingsFile();

            BindingsFileData combindedData = CombineBindings(data, localData);

            return localData;
        }

        private BindingsFileData CombineBindings(BindingsFileData firstData, BindingsFileData secondData)
        {
            secondData.UpdateAllUpdatedMethodsAndBindings(firstData);
            firstData.RemoveSharedData(secondData);

            secondData.AddMethodData(firstData.ProduceAllMethodData());

            return secondData;
        }

        internal string ProduceStringForStorage(BindingsFileData fileData, string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            string content = $"namespace SpecSharerLocalStorage{Environment.NewLine}{{{Environment.NewLine}    public class {fileName}{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}";

            content += fileData.ConvertToString();

            content += $"    }}{Environment.NewLine}}}";

            return content;
        }
    }
}
