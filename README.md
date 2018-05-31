# github-issues-cli

[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/jerriep/github-issues-cli/branch/master)

[appveyor-badge]: https://img.shields.io/appveyor/ci/jerriep/github-issues-cli/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/github-issues-cli/
[main-nuget-badge]: https://img.shields.io/nuget/v/github-issues-cli.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/jerriep/package/nuget/github-issues-cli
[main-myget-badge]: https://img.shields.io/www.myget/jerriep/vpre/github-issues-cli.svg?style=flat-square&label=myget


A simple command-line client for managing GitHub Issues.

## Installation

Download and install the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

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

### Determining the repository

As a general principal, **github-issues-cli** will try and determine the GitHub repository from the current directory. So for example, if you run the `ghi list` command to list issues, it will determine whether the current directory is a Git repository with a GitHub remote. If it is, it will limit the issues to that repository only.

Another important thing to note is that when **github-issues-cli** automatically discovers the GitHub repository from the current folder, and that repository is a [forked repo](https://help.github.com/articles/fork-a-repo/), it will not operate on the fork, but instead on the parent of the fork.

### Authentication

Before using the GitHub Issues CLI, you will need to [create a personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/). Be sure to request the `repo` and `user` scopes.

This token can be passed to commands using the `-t|--token` option, or alternatively it can be saved in a `GITHUB_ISSUES_CLI_TOKEN` environment variable.

### Listing issues

You can list issues using the `list` command:

```text
Usage: ghi list [options]

Options:
  -?|-h|--help            Show help information
  -a|--all                Display all issues, regardless of user.
  -r|--repo <REPOSITORY>  The repository to limit the issues to. By default the repository in current folder will be used.
  -u|--user <USER>        The user who the issues are related to. Defaults to the authenticated user.
  -R|--rel <RELATION>     The relation of the issues to the user. Possible values: Assigned (default), Created or Mentioned
  -s|--state <STATE>      The state of the issues. Possible values: Open (default), Closed or All)
  -t|--token <TOKEN>      Your GitHub Personal Access token
```

You can pass the repository for which you want to list issues by passing the `-r|--repo` option. The repository must be specified in the format _owner/name_, e.g. _jerriep/github-issues-cli_.

If no repository is specified, and you are running the command from a directory which contains a Git repository with a GitHub remote, it will limit the issues to that remote GitHub repository. Alternatively it will list issues for the current across all repositories.

### Viewing an issue

You can view details of an issue with the `view` command:

```text
Usage: ghi view [arguments] [options]

Arguments:
  Issue               The reference to the issue to view (in the format owner/repo#123). When running command from a directory containing a repository, only the issue number can be used.

Options:
  -?|-h|--help        Show help information
  -b|--browser        Opens the issue in the web browser.
  -t|--token <TOKEN>  Your GitHub Personal Access token
```

When inside a directory with a remote GitHub repository, you only need to specify the issue number, e.g.

```text
ghi view 8
```

Alternatively, you will need to specify the full reference to the issue in the format _owner/repo#number_, e.g.

```text
ghi view jerriep/github-issues-cli#8
```

The view command will print the information of the issue to the console. You can pass the `-b|--browser` option to open the issue in your web browser.

### Creating a new issue

You can create a new issue using the `new` command:

```text
Usage: ghi new [arguments] [options]

Arguments:
  Title of the issue

Options:
  -?|-h|--help            Show help information
  -a|--assign <ASSIGN>    GitHub user(s) to assign to the issue.
  -b|--body <BODY>        Body of the issue
  -l|--label <LABEL>      Label(s) to assign to the issue.
  -r|--repo <REPOSITORY>  The repository in which to create the new issue. By default the repository in current folder will be used.
  -t|--token <TOKEN>      Your GitHub Personal Access token
```

You can assign multiple users to an issue by passing the `-a|--assign` option multiple times, e.g.:

```text
ghi new "This is a test issue" -a user1 -a user2
```

You can follow the same pattern to specify multiple labels using the `-l|--label` option.

