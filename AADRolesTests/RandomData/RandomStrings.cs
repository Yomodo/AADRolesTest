using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppRolesTesting
{
    /// <summary>
    /// Returns a random string.
    /// </summary>
    public class RandomStrings : RandomDataBase<string>
    {
        /// <summary>
        /// The max length of the string being requested.
        /// </summary>
        private int _maxlength = 0;

        /// <summary>
        /// The min length of the string being requested.
        /// </summary>
        private int _minlength = 0;

        /// <summary>
        /// True or False indicating if right padding is needed
        /// </summary>
        private bool _padRight = false;

        /// <summary>
        /// The character cache
        /// </summary>
        private char[] _chars;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStrings"/> class.
        /// </summary>
        /// <param name="maxLength">The max length of the string to generate</param>
        public RandomStrings(int maxLength):this(maxLength, CharacterType.LowerCase | CharacterType.UpperCase)
        {
            this._maxlength = maxLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStrings" /> class.
        /// </summary>
        /// <param name="maxLength">The max length of the string</param>
        /// <param name="charType">The character type enumeration</param>
        public RandomStrings(int maxLength, CharacterType charType)
            : this(0, maxLength, charType, false)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStrings" /> class.
        /// </summary>
        /// <param name="maxLength">The max length of the string</param>
        /// <param name="charType">The character type enumeration</param>
        /// <param name="padRight">Apply padding to ensure string length</param>
        public RandomStrings(int maxLength, CharacterType charType, bool padRight)
            : this(0, maxLength, charType, padRight)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStrings" /> class.
        /// </summary>
        /// <param name="minLength">The min length of the string</param>
        /// <param name="maxLength">The max length of the string</param>
        /// <param name="charType">The character type enumeration</param>
        public RandomStrings(int minLength, int maxLength, CharacterType charType)
            : this(minLength, maxLength, charType, false)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStrings" /> class.
        /// </summary>
        /// <param name="minLength">The min length of the string</param>
        /// <param name="maxLength">The max length of the string</param>
        /// <param name="charType">The character type enumeration</param>
        /// <param name="padRight">Apply padding to ensure string length</param>
        public RandomStrings(int minLength, int maxLength, CharacterType charType, bool padRight)
        {
            this._minlength = minLength;
            this._maxlength = maxLength;
            this._chars = GetChars(charType);
            this._padRight = padRight;
        }

        /// <summary>
        /// Returns a random string
        /// </summary>
        /// <returns>A random string</returns>
        public override string GetRandom()
        {
            ////var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            ////var stringChars = new char[_maxlength];

            ////for (int i = 0; i < stringChars.Length; i++)
            ////{
            ////    stringChars[i] = chars[_random.Next(chars.Length)];
            ////}

            ////var finalString = new String(stringChars);
            ////return finalString;

            int actuallength = 0;
            if (this._minlength == this._maxlength)
            {
                actuallength = this._maxlength;
            }
            else
            {
                actuallength = RandomInteger.GetRandomInteger(this._minlength, this._maxlength + 1);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= actuallength - 1; i++)
            {
                sb.Append(this.GetRandomChar());
            }

            if (this._padRight)
            {
                return sb.ToString().PadRight(this._maxlength);
            }
            else
            {
                return sb.ToString();
            }
        }

        /// <summary>
        /// Provides the chars for the asked CharacterType
        /// </summary>
        /// <param name="cht">The CharacterType enumeration</param>
        /// <returns>A char array</returns>
        private static char[] GetChars(CharacterType cht)
        {
            char[] ch = null;
            if ((cht & CharacterType.Digit) > 0)
            {
                ch = ReziseAndAppendCharArray(ch, GetDigits());
            }

            if ((cht & CharacterType.UpperCase) > 0)
            {
                ch = ReziseAndAppendCharArray(ch, GetUpperCase());
            }

            if ((cht & CharacterType.LowerCase) > 0)
            {
                ch = ReziseAndAppendCharArray(ch, GetLowerCase());
            }

            if ((cht & CharacterType.Space) > 0)
            {
                char[] spc = { ' ' };
                ch = ReziseAndAppendCharArray(ch, spc);
            }

            if ((cht & CharacterType.Symbol) > 0)
            {
                char[] spc = GetChars("!-/*");
                ch = ReziseAndAppendCharArray(ch, spc);
            }

            return ch;
        }

        /// <summary>
        /// Merges the two provided arrays
        /// </summary>
        /// <param name="mainArray">The primary array</param>
        /// <param name="toBeCopiedArray">The input array</param>
        /// <returns>A resized char array</returns>
        private static char[] ReziseAndAppendCharArray(char[] mainArray, char[] toBeCopiedArray)
        {
            if (mainArray == null)
            {
                mainArray = toBeCopiedArray;
            }
            else
            {
                int oldLength = mainArray.Length;
                char[] newArray = new char[oldLength + toBeCopiedArray.Length];
                Array.Copy(mainArray, 0, newArray, 0, oldLength);
                Array.Copy(toBeCopiedArray, 0, newArray, oldLength, toBeCopiedArray.Length);
                mainArray = newArray;
            }

            return mainArray;
        }

        /// <summary>
        /// Randomizes and returns a provided char string. Used for symbols.
        /// </summary>
        /// <param name="charRange">String with chars</param>
        /// <returns>Randomized string</returns>
        private static char[] GetChars(string charRange)
        {
            string[] ss = charRange.Split('-');
            char ch1 = ss[0][0];
            char ch2 = ss[1][0];
            char[] ch = new char[Convert.ToInt32(ch2) - Convert.ToInt32(ch1) + 1];

            for (int i = Convert.ToInt32(ch1); i <= Convert.ToInt32(ch2); i++)
            {
                ch[i - Convert.ToInt32(ch1)] = Convert.ToChar(i);
            }

            return ch;
        }

        /// <summary>
        /// Returns all digits.
        /// </summary>
        /// <returns>A char array of digits</returns>
        private static char[] GetDigits()
        {
            char[] ch = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            return ch;
        }

        /// <summary>
        /// Returns all uppercase characters.
        /// </summary>
        /// <returns>A char array of uppercase alphabets</returns>
        private static char[] GetUpperCase()
        {
            char[] ch = new char[26];
            int asciivalueofA = Convert.ToInt32('A');

            for (int i = asciivalueofA; i < asciivalueofA + 26; i++)
            {
                ch[i - asciivalueofA] = Convert.ToChar(i);
            }

            return ch;
        }

        /// <summary>
        /// Returns all lowercase characters
        /// </summary>
        /// <returns>A char array of lowercase alphabets</returns>
        private static char[] GetLowerCase()
        {
            char[] ch = new char[26];
            int asciivalueofA = Convert.ToInt32('a');

            for (int i = asciivalueofA; i < asciivalueofA + 26; i++)
            {
                ch[i - asciivalueofA] = Convert.ToChar(i);
            }

            return ch;
        }

        /// <summary>
        /// Returns a random character
        /// </summary>
        /// <returns>a random character</returns>
        private char GetRandomChar()
        {
            int rnd = RandomInteger.GetRandomInteger(0, this._chars.GetUpperBound(0) + 1);
            return this._chars[rnd];
        }
    }
}