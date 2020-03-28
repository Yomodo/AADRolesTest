using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDCO.Ticketing.Client
{
    /// <summary>
    /// Generates random Boolean values
    /// </summary>
    /// <Author>Kalyan Krishna</Author>
    /// <Company>Spotless Pty Ltd.</Company>
    /// <Application>Buckeye v2</Application>
    /// <CopyRight>Copyright 2012 Microsoft</CopyRight>
    /// <DateCreated>Friday, August 10, 2012</DateCreated>
    public class RandomBoolean : RandomDataBase<bool>
    {
        /// <summary>
        /// Returns True or False randomly
        /// </summary>
        /// <returns>True or False</returns>
        public override bool GetRandom()
        {
            return _random.NextDouble() >= 0.5;
        }
    }
}
