using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace GDCO.Ticketing.Client
{
    /// <summary>
    /// Generates random enumerated values
    /// </summary>
    /// <typeparam name="T">The type of list</typeparam>
    /// <Author>Kalyan Krishna</Author>
    /// <Company>Spotless Pty Ltd.</Company>
    /// <Application>Buckeye v2</Application>
    /// <CopyRight>Copyright 2012 Microsoft</CopyRight>
    /// <DateCreated>Friday, August 10, 2012</DateCreated>
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
