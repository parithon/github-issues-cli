using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Octokit;
using Xunit;

namespace GitHubIssuesCli.Tests.Commands
{
    public class NewIssueCommandTests
    {
        private readonly Mock<IGitHubClient> _gitHubClient;
        private readonly Mock<IGitHubRepositoryDiscoveryService> _discoveryService;
        private readonly Mock<IIssuesClient> _issuesClient;
        private readonly Mock<IUsersClient> _usersClient; 
        private readonly Mock<IReporter> _reporter;

        private const string ValidOwner = "jerriep";
        private const string ValidRepo = "github-issues-cli";
        private const string InvalidRepo = "non-existent";
        private const string NewIssueTitle = "This is a new issue";
        private const string NewIssueBody = "New Issue Body";
        private const string ValidUser1 = "user1";
        private const string ValidUser2 = "user2";
        private const string InvalidUser = "invalid_user";
        
        public NewIssueCommandTests()
        {
            var repositoriesClient = new Mock<IRepositoriesClient>();
            repositoriesClient.Setup(client => client.Get(ValidOwner, ValidRepo))
                .ReturnsAsync(GitHubModelFactory.CreateRepository(ValidOwner, ValidRepo));
            repositoriesClient.Setup(client => client.Get(ValidOwner, InvalidRepo))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            _usersClient = new Mock<IUsersClient>();
            _usersClient.Setup(client => client.Get(ValidUser1))
                .ReturnsAsync(GitHubModelFactory.CreateUser(ValidUser1));
            _usersClient.Setup(client => client.Get(ValidUser2))
                .ReturnsAsync(GitHubModelFactory.CreateUser(ValidUser2));
            _usersClient.Setup(client => client.Get(InvalidUser))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            _issuesClient = new Mock<IIssuesClient>();
            _issuesClient.Setup(client => client.Create(ValidOwner, ValidRepo, It.IsAny<NewIssue>()))
                .ReturnsAsync(GitHubModelFactory.CreateIssue(ValidOwner, ValidRepo, 1));

            _gitHubClient = new Mock<IGitHubClient>();
            _gitHubClient.Setup(client => client.Repository)
                .Returns(repositoriesClient.Object);
            _gitHubClient.Setup(client => client.User)
                .Returns(_usersClient.Object);
            _gitHubClient.Setup(client => client.Issue)
                .Returns(_issuesClient.Object);

            _discoveryService = new Mock<IGitHubRepositoryDiscoveryService>();
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo(ValidOwner, ValidRepo));

            _reporter = new Mock<IReporter>();
        }
        
        [Fact]
        public async Task NotInARepoFolder_ReportsError()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task InvalidRepo_ReportsError()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Repository = $"{ValidOwner}/{InvalidRepo}";
            command.Title = NewIssueTitle;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task NewIssue_CallsGitHubApi()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.Create(ValidOwner, ValidRepo, It.Is<NewIssue>(issue => issue.Title == NewIssueTitle)));
        }

        [Fact]
        public async Task NewIssue_SetsBody()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            command.Body = NewIssueBody;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.Create(ValidOwner, ValidRepo, It.Is<NewIssue>(issue => issue.Body == NewIssueBody)));
        }

        [Fact]
        public async Task InvalidAssignee_ReportsError()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            command.Assign = new List<string> { InvalidUser };
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }
        
        [Fact]
        public async Task ValidAssignee_PassedToGitHub()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            command.Assign = new List<string> { ValidUser1, ValidUser2 };
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.Create(ValidOwner, ValidRepo, 
                It.Is<NewIssue>(issue => issue.Assignees.Contains(ValidUser1) && issue.Assignees.Contains(ValidUser2))));
        }

        [Fact]
        public async Task Labels_PassedToGitHub()
        {
            // Arrange
            NewIssueCommand command = new NewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Title = NewIssueTitle;
            command.Label = new List<string> { "label1", "label2" };
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.Create(ValidOwner, ValidRepo, 
                It.Is<NewIssue>(issue => issue.Labels.Contains("label1") && issue.Labels.Contains("label2"))));
        }
    }
}