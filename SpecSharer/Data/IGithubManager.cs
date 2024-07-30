using Octokit;
using SpecSharer.Logic;

namespace SpecSharer.Data
{
    public interface IGithubManager
    {
        string FileClass { get; set; }
        string FileNamespace { get; set; }

        Task<IReadOnlyList<RepositoryContent>> RetrieveFiles();
        Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path);
        Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path, string targetBranch);
        Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings);
        Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath);
        Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath, string targetBranch);
    }
}