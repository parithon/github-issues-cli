using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using GitHubIssuesCli.Commands;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace GitHubIssuesCli
{
    [Command(
        Name = "ghi", 
        FullName = "GitHub Issues Client",
        Description = "A command line utility to manage GitHub Issues")]
    //[Subcommand("auth", typeof(AuthCommand))]
    [Subcommand("list", typeof(ListIssuesCommand))]
    [Subcommand("view", typeof(ViewIssueCommand))]
    [Subcommand("new", typeof(NewIssueCommand))]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program: CommandBase
    {
        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IGitHubClient>(provider => new GitHubClient(new ProductHeaderValue("GitHub-Issues-CLI")))
                .AddSingleton<IConsole, PhysicalConsole>()
                .AddSingleton<IFileSystem, FileSystem>() 
                .AddSingleton<IGitHubRepositoryDiscoveryService, GitHubRepositoryDiscoveryService>()
                .AddSingleton<IBrowserService, BrowserService>()
                .AddSingleton<IReporter>(provider => new ConsoleReporter(provider.GetService<IConsole>()))
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.ThrowOnUnexpectedArgument = false;
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
         
            return app.Execute(args);
        }

        public static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
        
        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();

            return 3;
        }
    }
}

