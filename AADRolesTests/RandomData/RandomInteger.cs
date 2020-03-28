using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppRolesTesting
{
    /// <summary>
    /// Generates random integer values
    /// </summary>
    public class RandomInteger : RandomDataBase<int>
    {
        /// <summary>
        /// The minimum value
        /// </summary>
        private int _min;

        /// <summary>
        /// The maximum value
        /// </summary>
        private int _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomInteger" /> class.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public RandomInteger(int min, int max)
        {
            this._min = min;
            this._max = max;
        }

        /// <summary>
        /// Gets the random integer.
        /// </summary>
        /// <param name="onlyPositive">if set to <c>true</c> [only positive].</param>
        /// <returns>A random integer</returns>
        public static int GetRandomInteger(bool onlyPositive)
        {
            if (onlyPositive)
            {
                return GetRandomInteger(0, int.MaxValue);
            }
            else
            {
                return GetRandomInteger(int.MinValue, int.MaxValue);
            }
        }

        /// <summary>
        /// Gets the random integer.
        /// </summary>
        /// <returns>A random integer</returns>
        public static int GetRandomInteger()
        {
            return GetRandomInteger(true);
        }

        /// <summary>
        /// Gets the random integer.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>A random integer</returns>
        public static int GetRandomInteger(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Returns a random integer
        /// </summary>
        /// <returns>A random integer</returns>
        public override int GetRandom()
        {
            return _random.Next(this._min, this._max);
        }        
    }
}
