using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AppRolesTesting
{
    /// <summary>
    /// Provides random names to be used in unit tests
    /// </summary>
    public class RandomNames : RandomDataBase<string>
    {
        /// <summary>
        /// Master list of the various lists available
        /// </summary>
        private static Dictionary<NameType, List<string>> _nameLists = new Dictionary<NameType, List<string>>();

        /// <summary>
        /// Cached list of male names
        /// </summary>
        private static List<string> _maleNames = new List<string>();

        /// <summary>
        /// Cached list for female names
        /// </summary>
        private static List<string> _femaleNames = new List<string>();

        /// <summary>
        /// Cached list for words
        /// </summary>
        private static List<string> _words = new List<string>();

        /// <summary>
        /// The current word/name list
        /// </summary>
        private IList<string> _currentlist;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNames"/> class.
        /// </summary>
        /// <param name="type">The NameType(s).</param>
        public RandomNames(NameType type)
        {
            this._currentlist = RandomNames.GetListByNameTypes(type);
        }

        /// <summary>
        /// Gets the cached male names.
        /// </summary>
        private static List<string> MaleNames
        {
            get
            {
                if (_maleNames.Count == 0)
                {
                    FillListFromFile(_maleNames, NameType.MaleName);
                }

                return _maleNames;
            }
        }

        /// <summary>
        /// Gets the cached female names.
        /// </summary>
        private static List<string> FemaleNames
        {
            get
            {
                if (_femaleNames.Count == 0)
                {
                    FillListFromFile(_femaleNames, NameType.FemaleName);
                }

                return _femaleNames;
            }
        }

        /// <summary>
        /// Gets the cached words.
        /// </summary>
        private static List<string> Words
        {
            get
            {
                if (_words.Count == 0)
                {
                    FillListFromFile(_words, NameType.Word);
                }

                return _words;
            }
        }

        /// <summary>
        /// Gets the random name.
        /// </summary>
        /// <returns>A random name or word</returns>
        public override string GetRandom()
        {
            return this._currentlist[_random.Next(0, this._currentlist.Count)];
        }

        /// <summary>
        /// Gets the list by NameType(s) provided.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the list matching the name type</returns>
        private static List<string> GetListByNameTypes(NameType type)
        {
            if (!_nameLists.ContainsKey(type))
            {
                _nameLists.Add(type, GetMixedNameList(type));
            }

            return (List<string>)_nameLists[type];
        }

        /// <summary>
        /// Gets a list of words/names that correspond to the name types
        /// </summary>
        /// <param name="type">The NameType(s).</param>
        /// <returns>A combined list of all NameType(s)</returns>
        private static List<string> GetMixedNameList(NameType type)
        {
            List<string> combinedlist = new List<string>();

            if ((type & NameType.MaleName) == NameType.MaleName)
            {
                MergeLists(combinedlist, MaleNames);
            }

            if ((type & NameType.FemaleName) == NameType.FemaleName)
            {
                MergeLists(combinedlist, FemaleNames);
            }

            if ((type & NameType.Word) == NameType.Word)
            {
                MergeLists(combinedlist, Words);
            }

            return combinedlist;
        }

        /// <summary>
        /// Merges the two provided lists
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        private static void MergeLists(List<string> destination, List<string> source)
        {
            destination.AddRange(source);
        }

        /// <summary>
        /// Fills the list from embedded files.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="type">The type.</param>
        private static void FillListFromFile(List<string> list, NameType type)
        {
            switch (type)
            {
                case NameType.MaleName:
                    FillListFromResource(list, "RandomData.Resources.MaleName.txt", false);
                    break;

                case NameType.FemaleName:
                    FillListFromResource(list, "RandomData.Resources.FemaleName.txt", false);
                    break;

                case NameType.Word:
                    FillListFromResource(list, "RandomData.Resources.Word.txt", false);
                    break;
            }
        }

        /// <summary>
        /// Fills the list from resource files.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="truncate">if set to <c>true</c> [truncate].</param>
        private static void FillListFromResource(List<string> list, string resourceName, bool truncate)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            StreamReader sr = new StreamReader(assm.GetManifestResourceStream(resourceName));
            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine();

                if (truncate)
                {
                    line = line.Split(' ')[0];
                }

                list.Add(line);
            }

            sr.Close();
        }
    }
}