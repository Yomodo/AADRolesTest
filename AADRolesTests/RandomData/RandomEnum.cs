using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AppRolesTesting
{
    /// <summary>
    /// Generates random enumerated values
    /// </summary>
    /// <typeparam name="T">The type of list</typeparam>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText", Justification = "Stylecop has a bug")]
    public class RandomEnum<T> : RandomList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomEnum" /> class.
        /// </summary>
        public RandomEnum()
            : base(new List<T>((IList<T>)Enum.GetValues(typeof(T))))
        {
        }
    }
}
