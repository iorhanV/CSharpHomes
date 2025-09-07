// System
using System.Text;

// The class belongs to the utility namespace

namespace CSharpHomes.Utilities
{
    /// <summary>
    /// Methods of this class generally relate to string based operations.
    /// </summary>
    public static class StringUtils
    {
        #region String validation

        private static readonly List<char> CHARS_INVALID = new List<char>()
        {
            '/', '?', '<', '>', '\\', ':', '*', '|', '"', '^'
        };

        /// <summary>
        /// Replaces invalid characters in a string.
        /// </summary>
        /// <param name="checkString">The string to fix.</param>
        /// <param name="replaceChar">Character to substitute with (* = no substitute).</param>
        /// <returns>A string.</returns>
        public static string MakeStringValid(string checkString, char replaceChar = '*')
        {
            // New stringbuilder
            var newStringBuilder = new StringBuilder();

            // For each character
            foreach (char c in checkString)
            {
                // If the character is invalid
                if (CHARS_INVALID.Contains(c))
                {
                    // Replace if not using wildcard
                    if (replaceChar != '*')
                    {
                        newStringBuilder.Append(replaceChar);
                    }
                }
                else
                {
                    // Otherwise append it
                    newStringBuilder.Append(c);
                }
            }

            // Return the valid string
            return newStringBuilder.ToString();
        }

        #endregion
    }
}