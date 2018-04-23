namespace GitHubIssuesCli.Services
{
    public interface IGitHubRepositoryDiscoveryService
    {
        GitHubRepositoryInfo Discover(string path);
    }
}