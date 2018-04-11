using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            var github = new GitHubClient(new ProductHeaderValue("GitHub-Issues-CLI")) {Credentials = new Credentials(GitHubToken)};

            var user = await github.User.Current();

            console.Write("Listing open issues for ");
            console.Write($"@{user.Login}", ConsoleColor.DarkMagenta);
            console.WriteLine();
            console.WriteLine();
            
            var issues = await github.Issue.GetAllForCurrent();

            var groupedIssues = issues.GroupBy(i => i.Repository.FullName)
                .OrderBy(g => g.Key);

            foreach (var repo in groupedIssues)
            {
                console.WriteHeader(repo.Key);

                foreach (var issue in repo)
                {
                    console.WriteIndent(1);
                    console.Write($"#{issue.Number} ", ConsoleColor.DarkGreen);
                    console.Write(issue.Title);
                    console.Write($" @{issue.User.Login}", ConsoleColor.DarkMagenta);
                    console.WriteLine();
                }

                console.WriteLine();
            }

            return 0;
        }
    }

    public static class ConsoleExtensions
    {
        public static void Write(this IConsole console, object value, ConsoleColor color)
        {
            ConsoleColor currentColor = console.ForegroundColor;
            
            console.ForegroundColor = color;
            console.Write(value);
            console.ForegroundColor = currentColor;
        }
        
        public static void WriteIndent(this IConsole console, int level)
        {
            console.Write(new String(' ', level * 2));
        }
        
        public static void WriteHeader(this IConsole console, string value)
        {
            console.Write($"» {value}", ConsoleColor.DarkYellow);
            console.WriteLine();
        }
    }
}