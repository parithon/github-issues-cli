using System;
using Castle.Components.DictionaryAdapter;
using Octokit;

namespace GitHubIssuesCli.Tests
{
    public static class GitHubModelFactory
    {
        public static Issue CreateIssue(string owner, string repo, int number)
        {
            return new Issue(
                $"https://api.github.com/repos/{owner}/{repo}/issues/{number}",
                $"https://github.com/{owner}/{repo}/issues/{number}",
                $"https://api.github.com/repos/{owner}/{repo}/issues/{number}/comments",
                $"https://api.github.com/repos/{owner}/{repo}/issues/{number}/events",
                number,
                ItemState.Open,
                "Issue Title",
                "Issue Body",
                null,
                CreateUser(owner),
                null,
                null,
                null,
                null,
                0,
                null,
                null,
                DateTimeOffset.MinValue, 
                null,
                1,
                false,
                CreateRepository(owner, repo)
                );
        }

        public static Repository CreateRepository(string owner, string name)
        {
            return new Repository(
                $"https://api.github.com/repos/{owner}/{name}",
                $"https://github.com/{owner}/{name}",
                $"https://github.com/{owner}/{name}.git",
                $"git:github.com/{owner}/{name}.git",
                $"git@github.com:{owner}/{name}.git",
                $"https://svn.github.com/{owner}/{name}",
                $"git:git.example.com/{owner}/{name}",
                1,
                CreateUser(owner),
                name,
                $"{owner}/{name}",
                null,
                "https://github.com",
                null,
                false,
                false,
                0,
                0,
                "master",
                0,
                null,
                DateTimeOffset.MinValue,
                DateTimeOffset.MinValue,
                null,
                null,
                null,
                null,
                true,
                false,
                false,
                false,
                0,
                0,
                false,
                false,
                false
                );
        }

        public static User CreateUser(string login)
        {
            return new User(
                "https://github.com/images/error/octocat_happy.gif",
                null,
                null,
                0,
                null,
                DateTimeOffset.MinValue,
                DateTimeOffset.MinValue, 
                0,
                null,
                0,
                0,
                false,
                $"https://github.com/{login}",
                0,
                1,
                null,
                login,
                login,
                0,
                null,
                0,
                0,
                0,
                $"https://api.github.com/users/{login}",
                null,
                false,
                null,
                null
                );
        }
    }
}