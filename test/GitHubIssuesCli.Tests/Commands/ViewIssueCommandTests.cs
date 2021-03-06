﻿using System.IO.Abstractions;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Services;
using LibGit2Sharp;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using Octokit;
using Xunit;
using NotFoundException = Octokit.NotFoundException;

namespace GitHubIssuesCli.Tests.Commands
{
    public class ViewIssueCommandTests
    {
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
        
        public ViewIssueCommandTests()
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
        public async Task ValidationException_ReportsError()
        {
            // Arrange
            _discoveryService.Setup(service => service.DiscoverInCurrentDirectory())
                .Returns(() => null);

            ViewIssueCommand command = new ViewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _browserService.Object, _reporter.Object);
            command.Issue = $"{ValidOwner}/{InvalidRepo}#{ValidIssueNumber}";
            
            // Act
            await command.OnExecuteAsync(_console);

            // Assert
            _reporter.Verify(r => r.Error(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task PassingBrowserFlag_OpensBrowser()
        {
            // Arrange
            ViewIssueCommand command = new ViewIssueCommand(_gitHubClient.Object, _discoveryService.Object, _browserService.Object, _reporter.Object);
            command.Issue = $"{ValidIssueNumber}";
            
            // Act
            command.Browser = true;
            await command.OnExecuteAsync(_console);
            
            // Assert
            _browserService.Verify(service => service.OpenBrowser($"https://github.com/{ValidOwner}/{ValidRepo}/issues/{ValidIssueNumber}"), Times.Once);
        }

    }
}