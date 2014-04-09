using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DummyCollection
    {
        public static IList ReturnIList(IEnumerable<int> data)
        {
            return data.ToList();
        }

        public static IList AcceptIEnumerablOfIList(IEnumerable<IList> data)
        {
            return data.ToList();
        }

        public static IList AcceptIEnumerablOfIListInt(IEnumerable<IList<int>> data)
        {
            return data.ToList();
        }

        public static IList AcceptListOfList(List<List<int>> data)
        {
            return data;
        }

        public static IList Accept3DList(List<List<List<int>>> data)
        {
            return data;
        }

        public static IList<DummyPoint> ReturnListOf5Points()
        {
            return new List<DummyPoint> { new DummyPoint(){ X = 1, Y = 2, Z = 3}, 
                new DummyPoint(){ X = 2, Y = 2, Z = 3}, new DummyPoint(){ X = 3, Y = 2, Z = 3}, 
                new DummyPoint(){ X = 4, Y = 2, Z = 3}, new DummyPoint(){ X = 5, Y = 2, Z = 3}};
        }

        public static object AcceptListOf5PointsReturnAsObject(IEnumerable<DummyPoint> points)
        {
            return points;
        }

        public static IDictionary ReturnIDictionary()
        {
            IDictionary dictionary = new Dictionary<string, int>();
            dictionary.Add("A", 1);
            dictionary.Add("B", 2);
            dictionary.Add("C", 3);
            dictionary.Add("D", 4);
            return dictionary;
        }

        public static IDictionary AcceptDictionary(Dictionary<string, int> dictionary)
        {
            return dictionary;
        }
    }
}
