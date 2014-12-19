using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.DSASM
{
    public class DynamicFunctionTable
    {
        private List<DynamicFunction> functions;
        public IEnumerable<DynamicFunction> Functions 
        { 
            get
            {
                return functions;
            }
        }

        public DynamicFunctionTable()
        {
            functions = new List<DynamicFunction>();
        }

        public DynamicFunction AddNewFunction(string name, 
                                              int argumentNumber,
                                              int classIndex)
        {
            var func = new DynamicFunction(name, argumentNumber, classIndex);
            functions.Add(func);
            func.Index = functions.Count - 1;
            return func;
        }

        public DynamicFunction GetFunctionAtIndex(int index)
        {
            if (index < 0 || index >= functions.Count)
            {
                throw new ArgumentOutOfRangeException(/*NXLT*/"index", /*NXLT*/"Index is out of range.");
            }

            return functions[index];
        }

        public bool TryGetFunction(string name,
                                   int argumentNumber,
                                   int classIndex,
                                   out DynamicFunction func)
        {
            func = functions.FirstOrDefault(f => 
                        f.Name.Equals(name) &&
                        f.ArgumentNumber == argumentNumber &&
                        f.ClassIndex == classIndex);

            return func != null;
        }
    }

    /// <summary>
    /// It represents an unresolved function in the code. For any unresolved
    /// function, a DynamicFunction instance will be created and be added to
    /// DynamicFunctionTable. At runtime, callr will fetch the corresponding
    /// DynamicFunction instance from DynamicFunctionTable, and based on its 
    /// name/argument number/class scope to resolves function dynamically.
    /// </summary>
    public class DynamicFunction
    {
        public string Name { get; set; }
        public int ArgumentNumber { get; set; }
        public int ClassIndex { get; set; }
        public int Index { get; set; }

        public DynamicFunction(string name, int argumentNumber, int classIndex)
        {
            Name = name;
            ArgumentNumber = argumentNumber;
            ClassIndex = classIndex;
            Index = Constants.kInvalidIndex;
        }
    }
}
