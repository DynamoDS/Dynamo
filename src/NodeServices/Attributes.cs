using System;
using System.Collections;

namespace Autodesk.DesignScript.Runtime
{
    /// <summary>
    /// This attribute is used to specify whether the item will be imported
    /// into the VM.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class SupressImportIntoVMAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used to specify whether the item will be displayed
    /// in the library.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class IsVisibleInDynamoLibraryAttribute : Attribute
    {
        public IsVisibleInDynamoLibraryAttribute(bool visible)
        {
            Visible = visible;
        }

        public bool Visible { get; private set; }
    }

    /// <summary>
    /// This attribute is used to marshal parameters or return value of a method 
    /// as arbitrary dimension array in DesignScript VM. Usually this attribute
    /// is expected to be applied on IEnumerable derived object types. This 
    /// attribute should be used if you expect the IEnumerable parameters or
    /// return value may hold nested collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class ArbitraryDimensionArrayImportAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IExtensionApplication interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IExtensionApplication interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    internal sealed class ExtensionApplicationAttribute : Attribute
    {
        public ExtensionApplicationAttribute(Type entryPointType)
        {
            Type = entryPointType;
        }

        public Type Type { get; private set; }
    }

    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IGraphicDataProvider interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IGraphicDataProvider interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class GraphicDataProviderAttribute : Attribute
    {
        public GraphicDataProviderAttribute(Type providerType)
        {
            Type = providerType;
        }

        public Type Type { get; private set; }
    }
    
    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IContextDataProvider interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IContextDataProvider interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    internal sealed class ContextDataProviderAttribute : Attribute
    {
        private Func<bool> mCapturesData = () => true;

        /// <summary>
        /// Constructor to construct this attribute with a delegate to check
        /// whether this data provider captures data.
        /// </summary>
        /// <param name="dataProviderType">Type that implements 
        /// IContextDataProvider interface</param>
        /// <param name="capturesData">Delegate to check if the provider can
        /// capture data</param>
        public ContextDataProviderAttribute(Type dataProviderType, Func<bool> capturesData)
        {
            Type = dataProviderType;
            if(null != capturesData)
                mCapturesData = capturesData;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderType">Type that implements 
        /// IContextDataProvider interface</param>
        public ContextDataProviderAttribute(Type dataProviderType)
        {
            Type = dataProviderType;
        }

        /// <summary>
        /// Type implementing IContextDataProvider interface.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Checks if this type can capture data interactively
        /// </summary>
        public bool CapturesData 
        {
            get { return mCapturesData(); }
        }
    }

    /// <summary>
    /// This attribute can be applied to methods that return collection of
    /// objects, but with some combination of input parameters it returns a 
    /// collection of single object and at designscript side we want the method
    /// to return a single object instead of a collection of single object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class AllowRankReductionAttribute : Attribute
    {
        /// <summary>
        /// Utility method to get the single object from the collection of 
        /// single object. If the input object is neither a collection nor a
        /// collection of single object, this method returns the input object.
        /// </summary>
        /// <param name="collection">Input object to be converted to singleton.
        /// </param>
        /// <returns>An object from the collection of single object or the 
        /// input object.</returns>
        /// 
        public object ReduceRank(object collection)
        {
            if (null == collection)
                return null;

            Type type = collection.GetType();
            if (type.IsArray)
            {
                Array arr = collection as Array;
                if (null != arr && arr.Length == 1)
                    return arr.GetValue(0);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IEnumerable arr = collection as IEnumerable;
                if (null != arr)
                {
                    int count = 0;
                    object first = null;
                    foreach (var item in arr)
                    {
                        ++count;
                        if (count <= 1)
                            first = item;
                        else if (count > 1)
                            break;
                    }
                    if (count == 1)
                        return first;
                }
            }

            return collection;
        }

        /// <summary>
        /// Checks if the input object is a collection of single object.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsRankReducible(object collection)
        {
            if (null == collection)
                return false;

            Type type = collection.GetType();
            if (type.IsArray)
            {
                Array arr = collection as Array;
                if (null != arr && arr.Length == 1)
                    return true;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IEnumerable arr = collection as IEnumerable;
                if (null != arr)
                {
                    int count = 0;
                    foreach (var item in arr)
                    {
                        ++count;
                        if (count > 1)
                            return false;
                    }
                    if (count == 1)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// This attribute describes keys in the returned dictionary of a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MultiReturnAttribute: Attribute
    {
        public MultiReturnAttribute(params string[] returnKeys)
        {
            ReturnKeys = returnKeys;
        }

        public string[] ReturnKeys
        {
            get;
            set;
        }
    }

    /// <summary>
    /// This attribute can be applied to method which requires some runtime 
    /// support from DesignScript, e.g., tracing. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeRequirementAttribute: Attribute
    {
        public bool RequireTracing { get; set; }
    }

    /// <summary>
    /// This attribute can be applied to parameter to specify a default 
    /// argument expressions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class DefaultArgumentAttribute : Attribute
    {
        public string ArgumentExpression { get; private set; }
        public DefaultArgumentAttribute(string defaultArgumentExpression)
        {
            ArgumentExpression = defaultArgumentExpression;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class CanUpdatePeriodicallyAttribute : Attribute
    {
        public CanUpdatePeriodicallyAttribute(bool canUpdatePeriodically)
        {
            CanUpdatePeriodically = canUpdatePeriodically;
        }

        public bool CanUpdatePeriodically { get; private set; }
    }

    /// <summary>
    /// This attribute can be applied to class to give a hint for generating
    /// variable name in node to code. For example, generating variable "vec"
    /// for Vector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PreferredShortNameAttribute: Attribute
    {
        public string PreferredShortName { get; private set; }
        public PreferredShortNameAttribute(string preferredShortName)
        {
            PreferredShortName = preferredShortName;
        }
    }

    /// <summary> 
    /// This attribute indicates the node is obsolete
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method)] 
    public class IsObsoleteAttribute : Attribute 
    { 
        public string Message { get; protected set; } 
 
        public IsObsoleteAttribute() 
        { 
            Message = String.Empty; 
        } 
 
        public IsObsoleteAttribute(string message) 
        { 
            Message = message; 
        } 
    } 


    /// <summary>
    /// This attribute indicates the parameter will be referenced by the return
    /// object, hence its DS wrap object shouldn't be disposed even it is out
    /// of scope. The life-cycle of parameter will have the same life-cycle as
    /// the return object.
    /// 
    /// Note the type of return object should be reference type, either a
    /// pointer or an array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class KeepReferenceAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is applied to member function of zero touch libary.
    /// It indicates the return object should keep a reference to "this"
    /// object so that even "this" object is out of scope, it will not be
    /// disposed.
    ///
    /// Note the type of return object should be reference type, either a
    /// pointer or an array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class KeepReferenceThisAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is applied to a function to indicate whether to
    /// disable lacing strategy on this function or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IsLacingDisabledAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is applied to a function that has one or more parameters
    /// as lists. It can be used to control arguments to the function
    /// from being promoted to arrays or arrays of higher dimension when the VM tries
    /// to do method resolution and match argument(s) to the function parameter(s). 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowArrayPromotionAttribute : Attribute
    {
        public bool IsAllowed { get; private set; }

        public AllowArrayPromotionAttribute()
        {
            IsAllowed = true;
        }

        public AllowArrayPromotionAttribute(bool isAllowed)
        {
            IsAllowed = isAllowed;
        }
    }
}
