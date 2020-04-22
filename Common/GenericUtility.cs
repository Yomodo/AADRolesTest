using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class GenericUtility<T>
    {
        public static IList<T> BackupAddAndReplace(IEnumerable<T> source, IEnumerable<T> itemsToAdd)
        {
            List<T> toreturn = new List<T>(source.ToList());

            toreturn.AddRange(itemsToAdd);

            return toreturn;
        }

        public static IList<T> GetaRandomNumberOfItemsFromList(IEnumerable<T> sourceList, int numberofItems = 2)
        {
            List<T> toreturn = new List<T>();
            int totIterations = 0;

            if (sourceList == null || sourceList.Count() <= numberofItems)
            {
                return sourceList?.ToList();
            }

            Random random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);

            while (toreturn.Count() != numberofItems)
            {
                if (totIterations > 1000) break;

                int index = random.Next(0, sourceList.Count());

                T item = sourceList.ElementAtOrDefault(index);
                if (!toreturn.Contains(item))
                {
                    toreturn.Add(item);
                }

                totIterations++;
            }

            return toreturn;
        }
    }
}