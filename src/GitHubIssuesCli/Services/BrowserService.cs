using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GitHubIssuesCli.Services
{
    internal class BrowserService : IBrowserService
    {
        /// <remarks>
        /// Shamelessly stolen from https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/79a7afe6e45027b2ee14206a653436e5853f6b81/NetCoreConsoleClient/src/NetCoreConsoleClient/SystemBrowser.cs#L71-L97
        /// </remarks>
        public void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}