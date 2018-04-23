using System.IO.Abstractions;
using System.Threading.Tasks;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Services;
using LibGit2Sharp;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Octokit;
using Xunit;

namespace GitHubIssuesCli.Tests.Commands
{
    public class ViewIssueCommandTests
    {
        private readonly Mock<IGitHubClient> _gitHubClient;
        private readonly Mock<IGitHubRepositoryDiscoveryService> _discoveryService;
        private readonly Mock<IBrowserService> _browserService;
        private readonly Mock<IReporter> _reporter;
        private readonly Mock<IConsole> _console;

        private const string Owner = "jerriep";
        private const string Repo = "github-issues-cli";
        private const int IssueNumber = 100;
        
        public ViewIssueCommandTests()
        {
            _gitHubClient = new Mock<IGitHubClient>();
            _discoveryService = new Mock<IGitHubRepositoryDiscoveryService>();
            _browserService = new Mock<IBrowserService>();
            _reporter = new Mock<IReporter>();
            _console = new Mock<IConsole>();
        }
        
        [Fact]
        public async Task NotInARepoFolder_ReportsError()
        {
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ViewIssueCommand command = new ViewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _browserService.Object, _reporter.Object);
            command.Issue = $"{IssueNumber}";
            await command.OnExecuteAsync(_console.Object);

            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task NotInARepoFolder_OpensBrowser_WhenFullIssueSpecified()
        {
            var issuesClient = new Mock<IIssuesClient>();
            var repositoriesClient = new Mock<IRepositoriesClient>();

            repositoriesClient.Setup(client => client.Get(Owner, Repo))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository(Owner, Repo)));
            issuesClient.Setup(client => client.Get(Owner, Repo, IssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(Owner, Repo, IssueNumber)));

            _gitHubClient.Setup(client => client.Issue)
                .Returns(issuesClient.Object);
            _gitHubClient.Setup(client => client.Repository)
                .Returns(repositoriesClient.Object);
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo(Owner, Repo));

            ViewIssueCommand command = new ViewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _browserService.Object, _reporter.Object);
            command.Issue = $"{Owner}/{Repo}#{IssueNumber}";
            await command.OnExecuteAsync(_console.Object);

            _browserService.Verify(service => service.OpenBrowser($"https://github.com/{Owner}/{Repo}/issues/{IssueNumber}"), Times.Once);
        }

        [Fact]
        public async Task ViewValidIssue_OpensBrowser()
        {
            var issuesClient = new Mock<IIssuesClient>();
            var repositoriesClient = new Mock<IRepositoriesClient>();

            repositoriesClient.Setup(client => client.Get(Owner, Repo))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository(Owner, Repo)));
            issuesClient.Setup(client => client.Get(Owner, Repo, IssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(Owner, Repo, IssueNumber)));

            _gitHubClient.Setup(client => client.Issue)
                .Returns(issuesClient.Object);
            _gitHubClient.Setup(client => client.Repository)
                .Returns(repositoriesClient.Object);
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo(Owner, Repo));
            
            ViewIssueCommand command = new ViewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _browserService.Object, _reporter.Object);
            command.Issue = $"{IssueNumber}";
            await command.OnExecuteAsync(_console.Object);
            
            _browserService.Verify(service => service.OpenBrowser($"https://github.com/{Owner}/{Repo}/issues/{IssueNumber}"), Times.Once);
        }

    }
}