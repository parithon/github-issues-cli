    using System;
using System.Threading.Tasks.Dataflow;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    public static class ConsoleExtensions
    {
        public static void Write(this IConsole console, object value, ConsoleColor color)
        {
            ConsoleColor currentColor = console.ForegroundColor;
            
            console.ForegroundColor = color;
            console.Write(value);
            console.ForegroundColor = currentColor;
        }

        public static void WriteEmptyResult(this IConsole console, string value)
        {
            console.Write($"-- {value} --", ConsoleColor.DarkRed);
            console.WriteLine();
        }
        
        public static void WriteIndent(this IConsole console, int level)
        {
            console.Write(new String(' ', level * 2));
        }
        
        public static void WriteHeader(this IConsole console, string value)
        {
            console.Write($"» {value}", ConsoleColor.DarkYellow);
            console.WriteLine();
        }
    }
}