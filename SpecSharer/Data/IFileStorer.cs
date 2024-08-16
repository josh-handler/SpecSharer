using SpecSharer.Logic;

namespace SpecSharer.Storage
{
    internal interface IFileStorer
    {
        void StoreBindings(string path, BindingsFileData data, bool overwrite, IMethodReader reader);
    }
}