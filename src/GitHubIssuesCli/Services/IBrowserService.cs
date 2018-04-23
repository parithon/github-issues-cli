namespace GitHubIssuesCli.Services
{
    public interface IBrowserService
    {
        /// <remarks>
        /// Shamelessly stolen from https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/79a7afe6e45027b2ee14206a653436e5853f6b81/NetCoreConsoleClient/src/NetCoreConsoleClient/SystemBrowser.cs#L71-L97
        /// </remarks>
        void OpenBrowser(string url);
    }
}