﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDCO.Ticketing.Client
{
    /// <summary>
    /// Generates random double values
    /// </summary>
    /// <Author>Kalyan Krishna</Author>
    /// <Company>Spotless Pty Ltd.</Company>
    /// <Application>Buckeye v2</Application>
    /// <CopyRight>Copyright 2012 Microsoft</CopyRight>
    /// <DateCreated>Friday, August 10, 2012</DateCreated>
    public class RandomDouble : RandomDataBase<double>
    {
        /// <summary>
        /// Min double value
        /// </summary>
        private double _min;

        /// <summary>
        /// Max double value
        /// </summary>
        private double _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDouble" /> class.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public RandomDouble(double min, double max)
        {
            this._min = min;
            this._max = max;
        }

        /// <summary>
        /// Returns a random double
        /// </summary>
        /// <returns>A random double</returns>
        public override double GetRandom()
        {
            return ((this._max - this._min) * _random.NextDouble()) + this._min;
        }
    }
}
