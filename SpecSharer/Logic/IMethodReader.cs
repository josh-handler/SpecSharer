using Octokit;

namespace SpecSharer.Logic
{
    public interface IMethodReader
    {
        string GetFilePath();
        BindingsFileData ProcessBindingsFile();
        BindingsFileData ProcessBindingsFileFromRepository(RepositoryContent content);
        BindingsFileData ProcessString(string bindingsString);
        bool SetFilePath(string path);
    }
}