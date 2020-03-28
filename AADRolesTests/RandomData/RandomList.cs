using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AppRolesTesting
{
    /// <summary>
    /// Takes a list as input and returns a random value from this list
    /// </summary>
    /// <typeparam name="T">The type of list</typeparam>
    public class RandomList<T> : RandomDataBase<T>
    {
        /// <summary>
        /// The list instance
        /// </summary>
        private IList<T> _list = new List<T>();

        /// <summary>
        /// Only unique values to be returned
        /// </summary>
        private bool _unique = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomList{T}" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public RandomList(IList<T> list) : this(list, false, false)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomList{T}" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="makeLocalCopyOfList">if set to <c>true</c> [make local copy of list].</param>
        public RandomList(IList<T> list, bool unique, bool makeLocalCopyOfList)
        {
            this._unique = unique;

            if (unique && makeLocalCopyOfList)
            {
                // make a copy 
                this._list = new List<T>();

                foreach (T o in list)
                {
                    this._list.Add(o);
                }
            }
            else
            {
                this._list = list;
            }
        }

        /// <summary>
        /// When called, returns a random item from this list
        /// </summary>
        /// <returns>An item from the list</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the list is exhausted</exception>
        public override T GetRandom()
        {
            if (this._unique)
            {
                if (this._list.Count == 0)
                {
                    throw new InvalidOperationException("The list is exhausted. No more unique items could be returned.");
                }

                int index = _random.Next(0, this._list.Count);

                T o = this._list[index];
                this._list.RemoveAt(index);
                return o;
            }
            else
            {
                return this._list[_random.Next(0, this._list.Count)];
            }
        }
    }
}
