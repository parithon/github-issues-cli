using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    internal abstract class RequiresTokenCommandBase : CommandBase
    {
        [Option(CommandOptionType.SingleValue, Description = "Your GitHub Personal Access token")]
        public string Token { get; set;  }

        protected string GitHubToken => string.IsNullOrEmpty(Token)
            ? Environment.GetEnvironmentVariable(Constants.GitHubTokenEnvironmentVariable)
            : Token;

        internal ValidationResult OnValidate(ValidationContext context)
        {
            if (context.ObjectInstance is CommandLineApplication application)
            {
                var tokenOption = application.Options.FirstOrDefault(o => o.LongName == "token");
                if (tokenOption != null && tokenOption.Values.Count == 0)
                {
                    if (Environment.GetEnvironmentVariable(Constants.GitHubTokenEnvironmentVariable) == null)
                        return new ValidationResult("You need to specify a GitHub token");
                }
            }
            
            return ValidationResult.Success;
        }

    }
}