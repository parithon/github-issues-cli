﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class ListIssuesCommandTests
    {
        private readonly Mock<IGitHubClient> _gitHubClient;
        private readonly Mock<IGitHubRepositoryDiscoveryService> _discoveryService;
        private readonly Mock<IReporter> _reporter;
        private readonly Mock<IConsole> _console;
        private readonly Mock<IRepositoriesClient> _repositoriesClient;
        private readonly Mock<IIssuesClient> _issuesClient;
        private readonly Mock<IUsersClient> _usersClient;

        private const string ValidCurrentUser = "jerriep";
        private const string ValidUser = "valid-user";
        private const string InvalidUser = "invalid-user";
        private const string ValidOwner = "jerriep";
        private const string ValidRepo = "github-issues-cli";
        private const string InvalidRepo = "non-existent";
        private const int ValidIssueNumber = 100;
        
        public ListIssuesCommandTests()
        {
            _repositoriesClient = new Mock<IRepositoriesClient>();
            _repositoriesClient.Setup(client => client.Get(ValidOwner, ValidRepo))
                .Returns(Task.FromResult(GitHubModelFactory.CreateRepository(ValidOwner, ValidRepo)));
            _repositoriesClient.Setup(client => client.Get(ValidOwner, InvalidRepo))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            IReadOnlyList<Issue> issues = new List<Issue>();
            _issuesClient = new Mock<IIssuesClient>();
            _issuesClient.Setup(client => client.Get(ValidOwner, ValidRepo, ValidIssueNumber))
                .Returns(Task.FromResult(GitHubModelFactory.CreateIssue(ValidOwner, ValidRepo, ValidIssueNumber)));
            _issuesClient.Setup(client => client.GetAllForRepository(ValidOwner, ValidRepo, It.IsAny<RepositoryIssueRequest>()))
                .Returns(Task.FromResult(issues));
            _issuesClient.Setup(client => client.GetAllForCurrent(It.IsAny<IssueRequest>()))
                .Returns(Task.FromResult(issues));
            
            _usersClient = new Mock<IUsersClient>();
            _usersClient.Setup(client => client.Current())
                .Returns(Task.FromResult(GitHubModelFactory.CreateUser(ValidCurrentUser)));
            _usersClient.Setup(client => client.Get(ValidUser))
                .Returns(Task.FromResult(GitHubModelFactory.CreateUser(ValidUser)));
            _usersClient.Setup(client => client.Get(InvalidUser))
                .Throws(new NotFoundException("Say what!?", HttpStatusCode.NotFound));

            _gitHubClient = new Mock<IGitHubClient>();
            _gitHubClient.Setup(client => client.Issue)
                .Returns(_issuesClient.Object);
            _gitHubClient.Setup(client => client.Repository)
                .Returns(_repositoriesClient.Object);
            _gitHubClient.Setup(client => client.User)
                .Returns(_usersClient.Object);
            
            _discoveryService = new Mock<IGitHubRepositoryDiscoveryService>();
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => new GitHubRepositoryInfo(ValidOwner, ValidRepo));

            _reporter = new Mock<IReporter>();
            _console = new Mock<IConsole>();
        }
        
        [Fact]
        public async Task InsideRepoDirectory_QueriesIssuesForRepo()
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Repository = $"{ValidOwner}/{ValidRepo}";            
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, It.IsAny<RepositoryIssueRequest>()), Times.Once());
        }

        [Fact]
        public async Task PassedValidRepo_QueriesIssuesForRepo()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Repository = $"{ValidOwner}/{ValidRepo}";            
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, It.IsAny<RepositoryIssueRequest>()), Times.Once());
        }

        [Theory]
        [InlineData(ItemStateFilter.All)]
        [InlineData(ItemStateFilter.Closed)]
        [InlineData(ItemStateFilter.Open)]
        public async Task ForRepo_PassesState(ItemStateFilter state)
        {
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.State = state;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, It.Is<RepositoryIssueRequest>(request => request.State == state)), Times.Once());
        }

        [Theory]
        [InlineData(IssueRelation.Assigned)]
        [InlineData(IssueRelation.Created)]
        [InlineData(IssueRelation.Mentioned)]
        public async Task ForRepo_SetsRelation(IssueRelation relation)
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Relation = relation;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, 
                It.Is<RepositoryIssueRequest>(request => SetsCorrectRelation(request, relation, ValidCurrentUser))), Times.Once());
        }

        [Fact]
        public async Task ForRepo_WhenSpecifyingAll_ListForAllUsers()
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.All = true;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, 
                It.Is<RepositoryIssueRequest>(request => request.Assignee == null && request.Creator == null && request.Mentioned == null)), Times.Once());
        }

        [Fact]
        public async Task InvalidRepo_ReportsError()
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Repository = $"{ValidOwner}/{InvalidRepo}";            

            // Act
            await command.OnExecuteAsync(_console.Object);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task NoRepo_QueriesIssuesForCurrentUser()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForCurrent(It.IsAny<IssueRequest>()), Times.Once());
        }

        [Theory]
        [InlineData(ItemStateFilter.All)]
        [InlineData(ItemStateFilter.Closed)]
        [InlineData(ItemStateFilter.Open)]
        public async Task NoRepo_PassesState(ItemStateFilter state)
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.State = state;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForCurrent(It.Is<IssueRequest>(request => request.State == state)), Times.Once());
        }

        [Theory]
        [InlineData(IssueRelation.Assigned, IssueFilter.Assigned)]
        [InlineData(IssueRelation.Created, IssueFilter.Created)]
        [InlineData(IssueRelation.Mentioned, IssueFilter.Mentioned)]
        public async Task NoRepo_PassesRelation(IssueRelation relation, IssueFilter filter)
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.Relation = relation;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForCurrent(It.Is<IssueRequest>(request => request.Filter == filter)), Times.Once());
        }
        
        [Fact]
        public async Task NoRepo_WhenSpecifyingAll_ReportsWarning()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.All = true;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            Assert.False(command.All); // -all flag should be reset to false
            _reporter.Verify(reporter => reporter.Warn(It.IsAny<string>()), Times.Once());
        }


        [Fact]
        public async Task InvalidUser_ReportsError()
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.User = InvalidUser;       

            // Act
            await command.OnExecuteAsync(_console.Object);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }
 
        [Theory]
        [InlineData(IssueRelation.Assigned)]
        [InlineData(IssueRelation.Created)]
        [InlineData(IssueRelation.Mentioned)]
        public async Task ValidUser_SetsRelation(IssueRelation relation)
        {
            // Arrange
            ListIssuesCommand command = new ListIssuesCommand(_gitHubClient.Object, _discoveryService.Object, _reporter.Object);
            command.User = ValidUser;
            command.Relation = relation;
            
            // Act
            await command.OnExecuteAsync(NullConsole.Singleton);

            // Assert
            _issuesClient.Verify(client => client.GetAllForRepository(ValidOwner, ValidRepo, 
                It.Is<RepositoryIssueRequest>(request => SetsCorrectRelation(request, relation, ValidUser))), Times.Once());
        }
        
        #region Helper Methods

        private bool SetsCorrectRelation(RepositoryIssueRequest request, IssueRelation relation, string login)
        {
            switch (relation)
            {
                case IssueRelation.Assigned:
                    return request.Assignee == login;
                    break;
                case IssueRelation.Created:
                    return request.Creator == login;
                    break;
                case IssueRelation.Mentioned:
                    return request.Mentioned == login;
                    break;
            }

            return false;
        }
        

        #endregion
    }
}