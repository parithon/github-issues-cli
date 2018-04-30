using System.Net;
using System.Threading.Tasks;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Octokit;
using Xunit;

namespace GitHubIssuesCli.Tests.Commands
{
    public class CloseIssueCommandTests
    {
        private readonly Mock<IGitHubClient> _gitHubClient;
        private readonly Mock<IGitHubRepositoryDiscoveryService> _discoveryService;
        private readonly Mock<IBrowserService> _browserService;
        private readonly Mock<IReporter> _reporter;
        private readonly IConsole _console;
        private readonly Mock<IIssuesClient> _issuesClient;
        private readonly Mock<IIssueCommentsClient> _issueCommentsClient;

        private const string ValidOwner = "jerriep";
        private const string ValidRepo = "github-issues-cli";
        private const string InvalidRepo = "non-existent";
        private const int ValidIssueNumber = 100;
        private const int ClosedIssueNumber = 101;
        private const string ValidComment = "This is a comment";
        
        public CloseIssueCommandTests()
        {
            var repositoriesClient = new Mock<IRepositoriesClient>();
            repositoriesClient.Setup(client => client.Get(ValidOwner, ValidRepo))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository(ValidOwner, ValidRepo)));
            repositoriesClient.Setup(client => client.Get(ValidOwner, InvalidRepo))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            _issueCommentsClient = new Mock<IIssueCommentsClient>();

            _issuesClient = new Mock<IIssuesClient>();
            _issuesClient.SetupGet(client => client.Comment)
                .Returns(_issueCommentsClient.Object);
            _issuesClient.Setup(client => client.Get(ValidOwner, ValidRepo, ValidIssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(ValidOwner, ValidRepo, ValidIssueNumber)));
            _issuesClient.Setup(client => client.Get(ValidOwner, ValidRepo, ClosedIssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(ValidOwner, ValidRepo, ClosedIssueNumber, ItemState.Closed)));

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
        public async Task ValidationException_ReportsError()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{InvalidRepo}#{ValidIssueNumber}"
            };

            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ValidationException_ReturnsError()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{InvalidRepo}#{ValidIssueNumber}"
            };

            // Act
            var value = await command.OnExecuteAsync(_console);

            // Assert
            Assert.Equal(value, ReturnCodes.Error);
        }

        [Fact]
        public async Task ClosedIssue_ReportsWarning()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ClosedIssueNumber}"
            };

            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _reporter.Verify(r => r.Warn(It.Is<string>(s => s == "Issue jerriep/github-issues-cli#101 is already closed. No action taken.")), Times.Once());
        }

        [Fact]
        public async Task ClosedIssue_DoesNotUpdateIssue()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ClosedIssueNumber}"
            };

            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _issuesClient.Verify(client => client.Update(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IssueUpdate>()), Times.Never());
        }
        
        [Fact]
        public async Task ClosedIssue_ReturnsError()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ClosedIssueNumber}"
            };

            // Act
            var value = await command.OnExecuteAsync(_console);

            // Assert
            Assert.Equal(value, ReturnCodes.Error);
        }
        
        [Fact]
        public async Task Returns_OK()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ValidIssueNumber}"
            };

            // Act
            var value = await command.OnExecuteAsync(_console);

            // Assert
            Assert.Equal(value, ReturnCodes.Ok);
        }

        [Fact]
        public async Task OpenIssue_UpdatesIssue()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ValidIssueNumber}"
            };

            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _issuesClient.Verify(client => client.Update(ValidOwner, ValidRepo, ValidIssueNumber, It.Is<IssueUpdate>(update => update.State == ItemState.Closed)), Times.Once());
        }

        [Fact]
        public async Task PassingComment_AddsComment()
        {
            // Arrange
            var command = new CloseIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object)
            {
                Issue = $"{ValidOwner}/{ValidRepo}#{ValidIssueNumber}",
                Comment = ValidComment
            };

            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _issueCommentsClient.Verify(client => client.Create(ValidOwner, ValidRepo, ValidIssueNumber, ValidComment), Times.Once());
        }
    }
}