using System;
using System.Drawing;
using System.Globalization;

namespace GitHubIssuesCli
{
    public static class ConsoleColorHelper
    {        
        // https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor
        public static ConsoleColor FromHex(string hex)
        {
            int argb = Int32.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
            Color c = Color.FromArgb(argb);
            
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit
            
            return (System.ConsoleColor)index;
        }
    }
}