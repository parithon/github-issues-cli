using System;
using System.Net;
using System.Threading.Tasks;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Octokit;
using Xunit;

namespace GitHubIssuesCli.Tests.Commands
{
    public class GitHubIssueCommandBaseTests
    {
        /// <summary>
        /// Simple tester class to exercise the <see cref="GitHubIssueCommandBase"/> class.
        /// </summary>
        private class GitHubIssueCommandBaseTester : GitHubIssueCommandBase
        {
            public GitHubIssueCommandBaseTester(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService) 
                : base(gitHubClient, gitHubRepositoryDiscoveryService)
            {
            }

            public Task<(Issue, Repository)> GetIssueTesterAsync(string issueReference)
            {
                return GetIssueAsync(issueReference);
            }
        }
        
        private readonly Mock<IGitHubClient> _gitHubClient;
        private readonly Mock<IGitHubRepositoryDiscoveryService> _discoveryService;
        private readonly Mock<IBrowserService> _browserService;
        private readonly Mock<IReporter> _reporter;
        private readonly IConsole _console;
        private readonly Mock<IIssuesClient> _issuesClient;

        private const string ValidOwner = "jerriep";
        private const string ValidRepo = "github-issues-cli";
        private const string InvalidRepo = "non-existent";
        private const int ValidIssueNumber = 100;

        public GitHubIssueCommandBaseTests()
        {
            var repositoriesClient = new Mock<IRepositoriesClient>();
            repositoriesClient.Setup(client => client.Get(ValidOwner, ValidRepo))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository(ValidOwner, ValidRepo)));
            repositoriesClient.Setup(client => client.Get(ValidOwner, InvalidRepo))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            _issuesClient = new Mock<IIssuesClient>();
            _issuesClient.Setup(client => client.Get(ValidOwner, ValidRepo, ValidIssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(ValidOwner, ValidRepo, ValidIssueNumber)));

            _gitHubClient = new Mock<IGitHubClient>();
            _gitHubClient.Setup(client => client.Issue)
                .Returns(_issuesClient.Object);
            _gitHubClient.Setup(client => client.Repository)
                .Returns(repositoriesClient.Object);
            
            _discoveryService = new Mock<IGitHubRepositoryDiscoveryService>();
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo(ValidOwner, ValidRepo));

            _browserService = new Mock<IBrowserService>();
            _reporter = new Mock<IReporter>();
            _console = NullConsole.Singleton;
        }
        
        [Fact]
        public async Task NotInARepoFolder_ThrowsValidationException()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            var command = new GitHubIssueCommandBaseTester(_gitHubClient.Object, _discoveryService.Object);
            
            // Assert
            await Assert.ThrowsAsync<CommandValidationException>(() => command.GetIssueTesterAsync($"{ValidIssueNumber}"));
        }

        [Fact]
        public async Task NotInARepoFolder_RetrievesIssue_WhenFullIssueSpecified()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            var command = new GitHubIssueCommandBaseTester(_gitHubClient.Object, _discoveryService.Object);
            
            // Act
            await command.GetIssueTesterAsync($"{ValidOwner}/{ValidRepo}#{ValidIssueNumber}");

            // Assert
            _issuesClient.Verify(client => client.Get(ValidOwner, ValidRepo, ValidIssueNumber), Times.Once());
        }
        
        [Fact]
        public async Task InARepoFolder_RetrievesIssue_WhenIssueNumberSpecified()
        {
            // Arrange
            var command = new GitHubIssueCommandBaseTester(_gitHubClient.Object, _discoveryService.Object);
            
            // Act
            await command.GetIssueTesterAsync($"{ValidIssueNumber}");
            
            // Assert
            _issuesClient.Verify(client => client.Get(ValidOwner, ValidRepo, ValidIssueNumber), Times.Once());
        }


        [Fact]
        public async Task InvalidRepo_ReportsError()
        {
            // Arrange
            var command = new GitHubIssueCommandBaseTester(_gitHubClient.Object, _discoveryService.Object);
            
            // Act

            // Assert
            await Assert.ThrowsAsync<CommandValidationException>(() => command.GetIssueTesterAsync($"{ValidOwner}/{InvalidRepo}#{ValidIssueNumber}"));
        }
    }
}