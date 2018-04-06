using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace GitHubIssuesCli
{
    [Command(
        Name = "ghi", 
        FullName = "GitHub Issues Client",
        Description = "A simple command line utility to manage GitHub Issues")]
    //[Subcommand("auth", typeof(AuthCommand))]
    [Subcommand("list", typeof(ListIssuesCommand))]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program: CommandBase
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

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

