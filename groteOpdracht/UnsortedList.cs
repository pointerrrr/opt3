using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groteOpdracht
{
    static class UnsortedList
    {
        public static void UnsortedRemove<T>(this List<T> list, int index)
        {
            var node = list[list.Count -1];
            list[index] = node;
            list.RemoveAt(list.Count - 1);
        }
    }
}
