using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCore
{
    public static class Life
    {
        private static bool NaturalSelection(int numNeighbors, bool old)
        {
            return numNeighbors == 3 || numNeighbors == 2 && old;
        }

        public static IEnumerable<bool> Step(bool[] cells, int size)
        {
            var s2 = size*size;
            var offsets = new[] { -1 - size, -size, 1 - size, -1, 1, size - 1, size, 1 + size };
            return
                cells.Select(
                    (x, i) =>
                        NaturalSelection(offsets.Select(o => cells[Math.Abs((i + o)%s2)] ? 1 : 0).Sum(), x));
        }
    }
}
