# github-issues-cli

[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/jerriep/github-issues-cli/branch/master)

[appveyor-badge]: https://img.shields.io/appveyor/ci/jerriep/github-issues-cli/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/github-issues-cli/
[main-nuget-badge]: https://img.shields.io/nuget/v/github-issues-cli.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/github-issues-cli/package/nuget/github-issues-cli
[main-myget-badge]: https://img.shields.io/www.myget/github-issues-cli/vpre/github-issues-cli.svg?style=flat-square&label=myget


A simple command-line client for managing GitHub Issues.

## Installation

The latest release of github-issues-cli requires the [2.1.300-preview2](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300-preview2) .NET Core SDK or newer.

Once installed, run this command:

```bash
dotnet tool install --global github-issues-cli
```

## Usage

```text
Usage: ghi [options] [command]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

Commands:
  close         Closes a GitHub Issue
  list          List GitHub Issues
  new           Creates a new GitHub Issue
  open          Opens a GitHub Issue
  view          View details of specific GitHub Issue
```

### General usage

### Authentication

Before using the GitHub Issues CLI, you will need to [create a personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/). This token can be passed to commands using the `-t|--token` option, or alternatively it can be saved in a `GITHUB_ISSUES_CLI_TOKEN` environment variable.

### Listing issues

### Viewing an issue