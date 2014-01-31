using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace String
{
    public class String
    {

        public static int StringLength(string str)
        {
            return str.Length;
        }

        public static string ToUpper(string str)
        {
            return str.ToUpper();
        }

        public static string ToLower(string str)
        {
            return str.ToLower();
        }

        public static double ToNumber(string str)
        {
            return Convert.ToDouble(str);
        }

        public static List<string> SplitString(string str, string delimiter)
        {
            string[] strArray = { delimiter };
            string[] resultArray = str.Split(strArray, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> resultList = new List<string>();
            foreach (string s in resultArray)
            {
                resultList.Add(s);
            }
            return resultList;
        }

        public static string JoinStrings(List<string> str, string delimiter)
        {
            string result = "";
            int i = 0;
            for (; i < str.Count - 1; i++)
            {
                result = result + str[i] + delimiter;
            }
            result = result + str[i];
            return result;
        }

        public static string JoinStrings(List<string> str)
        {
            return JoinStrings(str, "");
        }

        public static string SubString(string str, int start, int length)
        {
            return str.Substring(start, length);
        }
    }
}