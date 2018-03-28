using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    internal abstract class RequiresTokenCommandBase : CommandBase
    {
        protected string GitHubAccessToken { get; private set; }

        public ValidationResult OnValidate()
        {
            SettingsStore store = new SettingsStore();
            if (!store.ContainsKey("GitHubAccessToken"))
                return new ValidationResult("You must authenticate with GitHub first");

            return ValidationResult.Success;
        }
    }
}