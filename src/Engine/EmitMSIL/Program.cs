using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;

namespace EmitMSIL
{
    //class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        Dictionary<string, IList> input = new Dictionary<string, IList>();
    //        input.Add("x", new[] { 12, 12.7 });
    //        input.Add("y", new[] { new[] { 12.3 } });
    //        input.Add("z", new[] { 453 });

    //        Dictionary<string, IList> output = new Dictionary<string, IList>();
    //        Execute.Exec(input, output);
    //    }
    //}

    public class ProgramExec
    {
        public static void Exec(string[] args)
        {
            Dictionary<string, IList> input = new Dictionary<string, IList>();
            input.Add("x", new[] { 12, 12.7 });
            input.Add("y", new[] { new[] { 12.3 } });
            input.Add("z", new[] { 453 });

            Dictionary<string, IList> output = new Dictionary<string, IList>();
            var methodCache = new Dictionary<int, IEnumerable<MethodBase>>();
            ExecuteIL.Execute(input, methodCache, output);
        }
    }
}
