namespace GitHubIssuesCli.Services
{
    public class GitHubRepositoryInfo
    {
        public string Owner { get; }

        public string Name { get; }

        public GitHubRepositoryInfo(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }
    }
}