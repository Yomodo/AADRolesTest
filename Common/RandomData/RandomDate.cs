using System;

namespace Common
{
    /// <summary>
    /// Generates random date values
    /// </summary>
    public class RandomDate : RandomDataBase<DateTime>
    {
        /// <summary>
        /// Min date
        /// </summary>
        private long _minDate = DateTime.MinValue.Ticks;

        /// <summary>
        /// Max date
        /// </summary>
        private long _maxDate = DateTime.Now.Ticks;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDate" /> class.
        /// </summary>
        public RandomDate() : this(DateTime.MinValue, DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDate" /> class.
        /// </summary>
        /// <param name="minDate">The min date.</param>
        public RandomDate(DateTime minDate) : this(minDate, DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDate" /> class.
        /// </summary>
        /// <param name="minDate">The min date.</param>
        /// <param name="maxDate">The max date.</param>
        public RandomDate(DateTime minDate, DateTime maxDate)
        {
            this._minDate = minDate.Ticks;
            this._maxDate = maxDate.Ticks;
        }

        /// <summary>
        /// Returns a random date
        /// </summary>
        /// <returns>A random date</returns>
        public override DateTime GetRandom()
        {
            return new DateTime((long)((_random.NextDouble() * (this._maxDate - this._minDate)) + this._minDate));
        }
    }
}