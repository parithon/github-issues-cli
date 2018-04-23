using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitHubIssuesCli.Services
{
    public class GitHubRepositoryDiscoveryService : IGitHubRepositoryDiscoveryService
    {
        public GitHubRepositoryInfo Discover(string path)
        {
            // Get the path for the git repo from the path 
            string repoPath = LibGit2Sharp.Repository.Discover(path);
            
            if (!string.IsNullOrEmpty(repoPath))
            {
                // Instantiate a Repository object from the path
                var repository = new LibGit2Sharp.Repository(repoPath);
                
                // Now from the remote, get the origin
                // TODO: Much more logic is required here. We assume origin, but it can be called something else. Also, how do we handle forked repos, because we would want to rather list the issues on the upstream, yeah?  
                var remote = repository.Network.Remotes.FirstOrDefault(r => r.Name == "origin");
                if (remote != null)
                {
                    // Create a URI instance, and then grab the first 2 segments as the user and repository respectively
                    string remoteUrl = remote.Url;
                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        // We need to do some normalization on the URL
                        // 1. Remove .git at the end
                        remoteUrl = Regex.Replace(remoteUrl, @"\.git$", "");

                        // 2. Normalize git@ and https:git@ urls
                        remoteUrl = Regex.Replace(remoteUrl, "^git@", "https://");
                        remoteUrl = Regex.Replace(remoteUrl, "^https:git@", "https://");
                        remoteUrl = Regex.Replace(remoteUrl, ".com:", ".com/");
                        
                        Uri remoteUri = new Uri(remoteUrl);
                        
                        // Check that the host is github.com
                        if (remoteUri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
                        {
                            // Now let's split the path
                            string[] pathSegments = remoteUri.AbsolutePath.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries);
                            
                            // Now we should be left with (at least) 2 entries, containing the user and the repo
                            if (pathSegments != null && pathSegments.Length >= 2)
                            {
                                return new GitHubRepositoryInfo(pathSegments[0], pathSegments[1]);
                            }
                        }                    
                    }
                }
            }   
            
            return null;
        }
    }
}