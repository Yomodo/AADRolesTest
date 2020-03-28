using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRolesTesting
{
    /// <summary>
    /// Enables Writing to console in various different colours
    /// </summary> 
    public static class ColorConsole
    {
        /// <summary>
        /// Writes the line in standard white.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="args">The args.</param>
        public static void WriteLine(string text, params object[] args)
        {
            WriteLine(ConsoleColor.White, text, args);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The args.</param>
        public static void WriteLine(ConsoleColor color, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// Writes to console
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The args.</param>
        public static void Write(ConsoleColor color, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text, args);
            Console.ForegroundColor = currentColor;
        }
    }
}
