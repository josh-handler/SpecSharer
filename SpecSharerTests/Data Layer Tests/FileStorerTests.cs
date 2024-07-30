using SpecSharer.Logic;
using SpecSharer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests
{
    public class FileStorerTests
    {
        readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        readonly string newTargetFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\TargetFile.txt");

        readonly string existingTargetFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\ExistingTargetFile.txt");

        private MethodReader reader;
        private FileStorer storer;

        private readonly ITestOutputHelper testOutputHelper;

        public FileStorerTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            storer = new FileStorer();
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void StoreBindingInNewFile()
        {
            File.Delete(newTargetFilePath);

            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            storer.StoreBindings(newTargetFilePath, fileData, true);

            string result = File.ReadAllText(newTargetFilePath);

            File.Delete(newTargetFilePath);

            Assert.Equal(fileData.ConvertToString(), result);
        }

        [Fact]
        public void StoreBindingInExistingFile()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            storer.StoreBindings(existingTargetFilePath, fileData, true);
            Assert.Equal($"Text Already Present In File{Environment.NewLine}{Environment.NewLine}{fileData.ConvertToString()}", File.ReadAllText(existingTargetFilePath));
        }
    }
}
