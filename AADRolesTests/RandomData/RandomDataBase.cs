using System;
using System.Diagnostics.CodeAnalysis;

namespace AppRolesTesting
{
    /// <summary>
    /// Base class for all random data generators
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public abstract class RandomDataBase<T>
    {
        ////protected Random _random = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));

        /// <summary>
        /// Internal random instance
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "It is required as such by the derived classes.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "It is required as such by the derived classes.")]
        protected static Random _random = new Random(System.DateTime.Now.Millisecond * System.DateTime.Now.Second);

        /// <summary>
        /// Returns a random instance of the type.
        /// </summary>
        /// <returns>a random value</returns>
        public abstract T GetRandom();
    }
}