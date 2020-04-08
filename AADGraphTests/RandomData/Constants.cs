using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: CLSCompliant(false)]

namespace AADGraphTesting
{
    /// <summary>
    /// The character type when generating random strings
    /// </summary>
    [Flags]
    public enum CharacterType
    {
        /// <summary>
        /// Can have spaces
        /// </summary>
        Space = 1,

        /// <summary>
        /// Can have digits
        /// </summary>
        Digit = 2,

        /// <summary>
        /// Can have upper case characters
        /// </summary>
        UpperCase = 4,

        /// <summary>
        /// Can have lower case characters
        /// </summary>
        LowerCase = 8,

        /// <summary>
        /// Can have symbols
        /// </summary>
        Symbol = 16
    }

    /// <summary>
    /// Random String generation types
    /// </summary>
    [Flags]
    public enum NameType
    {
        /// <summary>
        /// Male name
        /// </summary>
        MaleName = 1,

        /// <summary>
        /// Female names
        /// </summary>
        FemaleName = 2,

        /// <summary>
        /// Any word
        /// </summary>
        Word = 4
    }
}
