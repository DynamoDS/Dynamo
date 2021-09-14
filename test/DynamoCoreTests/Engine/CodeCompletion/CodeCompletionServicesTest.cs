using System;
using System.Linq;
using Dynamo.Engine.CodeCompletion;
using NUnit.Framework;
using ProtoCore.Mirror;
using ProtoCore.Namespace;
using ProtoCore.Utils;

namespace Dynamo.Tests.Engine.CodeCompletion
{
    [TestFixture]
    class CodeCompletionServicesTest
    {
        private ProtoCore.Core libraryServicesCore;

        [SetUp]
        public void Init()
        {
            string libraryPath = "FFITarget.dll";

            var options = new ProtoCore.Options();
            options.RootModulePathName = string.Empty;

            libraryServicesCore = new ProtoCore.Core(options);
            libraryServicesCore.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(libraryServicesCore));
            libraryServicesCore.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(libraryServicesCore));

            CompilerUtils.TryLoadAssemblyIntoCore(libraryServicesCore, libraryPath);
        }


        [TearDown]
        public void Cleanup()
        {
            if (libraryServicesCore != null)
            {
                libraryServicesCore.Cleanup();
                libraryServicesCore = null;
            }
        }

        /// <summary>
        /// This test method will execute the next methods from the CodeCompletionServices class:
        /// GetCompletionsOnType(string code, string stringToComplete, ElementResolver resolver = null)      
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CodeCompletionServicesGetCompletionsOnTypeTest()
        {
            //Arrange
            string ffiTargetClass = "CodeCompletionClass";
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            //We pass a ElementResolver parameter in the third parameter so will execute a specific section of the code
            var completions = codeCompletionServices.GetCompletionsOnType("", "CodeCompletionClass", new ElementResolver());
            var completionData = completions.First();
            completionData.Description= "Test";

            //Assert
            //Due that we are passing null in the parameter mirror will raise a ArgumentException
            Assert.Throws<ArgumentException>( () => CompletionData.ConvertMirrorToCompletionData(null));

            //Act
            CompletionData.ConvertMirrorToCompletionData(new ClassMirror(ffiTargetClass, libraryServicesCore), true, new ElementResolver());

            //Assert
            Assert.AreEqual(8, completions.Count());
        }

        /// <summary>
        /// This test method will execute the next methods from the CodeCompletionServices class:
        /// GetFunctionSignatures(string code, string functionName, string functionPrefix, ElementResolver resolver = null)
        /// static CompletionData ConvertMirrorToCompletionData(StaticMirror mirror, bool useShorterName = false,  ElementResolver resolver = null)
        /// static string GetShortClassName(ClassMirror mirror, ElementResolver resolver)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CodeCompletionServicesGetFunctionSignatures()
        {
            //Arrange
            string ffiTargetClass = "CodeCompletionClass";
            string functionName = "foo";
            string code = @"x[y[z.foo()].goo(";
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            //Act
            //We pass an empty string in the third parameter (functionPrefix) so will execute a specific section of the code
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, "");

            //Assert
            Assert.IsNotNull(overloads);

            //Act
            //We pass a ElementResolver parameter in the fourth parameter so will execute a specific section of the code
            overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, ffiTargetClass, new ElementResolver());

            //Assert
            Assert.IsNotNull(overloads);
            Assert.AreEqual(overloads.Count(), 0);

        }


        [Test]
        public void Constructor_WithIsVisibleFalseAttribute_HiddenFromSearch()
        {
            var className = @"FFITarget.DesignScript.Point";

            var cm = new ClassMirror(className, libraryServicesCore);
            var ctors = cm.GetConstructors();
            var ctorNames = ctors.Select(x => x.MethodName);
            var found = false;
            foreach(var name in ctorNames)
            {
                if (name == nameof(FFITarget.DesignScript.Point)) found = true;
            }
            Assert.False(found);
        }
    }
}
