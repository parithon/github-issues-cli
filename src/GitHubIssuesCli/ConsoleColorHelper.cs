using System;
using System.Drawing;
using System.Globalization;

namespace GitHubIssuesCli
{
    public static class ConsoleColorHelper
    {        
        // https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor
        // https://stackoverflow.com/questions/1855884/determine-font-color-based-on-background-color
        public static (ConsoleColor ForegroundColor, ConsoleColor BackgroundCololr) FromHex(string hex)
        {
            int argb = Int32.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
            Color c = Color.FromArgb(argb);
            
            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - ( 0.299 * c.R + 0.587 * c.G + 0.114 * c.B)/255;

            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit
            
            ConsoleColor backgroundColor = (System.ConsoleColor)index;
            ConsoleColor foregroundColor = a < 0.5 ? ConsoleColor.Black : ConsoleColor.White;
            
            return (foregroundColor, backgroundColor);
        }
    }
}