namespace GitHubIssuesCli
{
    public interface IGitHubRepositoryDiscoveryService
    {
        GitHubRepositoryInfo Discover(string path);
    }
}