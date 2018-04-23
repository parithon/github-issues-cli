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
        [Fact]
        public async Task ViewValidIssue_OpensBrowser()
        {
            var gitHubClient = new Mock<IGitHubClient>();
            var issuesClient = new Mock<IIssuesClient>();
            var repositoriesClient = new Mock<IRepositoriesClient>();
            var discoveryService = new Mock<IGitHubRepositoryDiscoveryService>();
            var browserService = new Mock<IBrowserService>();
            var reporter = new Mock<IReporter>();
            var console = new Mock<IConsole>();

            gitHubClient.Setup(client => client.Issue)
                .Returns(issuesClient.Object);
            gitHubClient.Setup(client => client.Repository)
                .Returns(repositoriesClient.Object);
            repositoriesClient.Setup(client => client.Get("jerriep", "github-issues-cli"))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository("jerriep", "github-issues-cli")));
            issuesClient.Setup(client => client.Get("jerriep", "github-issues-cli", 100))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue("jerriep", "github-issues-cli", 100)));
            discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo("jerriep", "github-issues-cli"));
            

            ViewIssueCommand command = new ViewIssueCommand(gitHubClient.Object, discoveryService.Object, browserService.Object, reporter.Object);
            command.Issue = "100";
            await command.OnExecuteAsync(console.Object);
            
            browserService.Verify(service => service.OpenBrowser("https://github.com/jerriep/github-issues-cli/issues/100"), Times.Once);
        }
    }
}