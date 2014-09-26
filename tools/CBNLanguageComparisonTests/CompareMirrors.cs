using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Dynamo;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using System.Linq;
using ProtoTestFx;
using ProtoTest.TD;
using ProtoTestFx.TD;
using Dynamo.DSEngine;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Text;
using ProtoCore.Utils;
using ProtoFFI;


namespace Dynamo.Tests
{
    /// <summary>
    /// The testframework to take the DesignScript code and dump into CBN for evaluation through Dynamo
    /// 
    /// </summary>

    public class CBNEngineTests : DSEvaluationViewModelUnitTest
    {
        

        private TestFrameWork thisTest = new TestFrameWork();
        
        public Guid RunDSScriptInCBN(string dscode)
        {
            /// Method that takes the DesignScript language code and dumps into CBN.
            /// Then evaluate in Dynamo and return the guid of the CBN
            var guid = Guid.NewGuid();
            var command1 = new DynamoViewModel.CreateNodeCommand(guid, "Code Block", 0, 0, false, false);

            ViewModel.ExecuteCommand(command1);
            var command2 = new DynamoViewModel.UpdateModelValueCommand(guid, "Code", dscode);
            ViewModel.ExecuteCommand(command2);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            return guid;
        }
        internal void CompareCores(ProtoCore.Core c1, ProtoCore.Core c2,Guid guid)
        {

            for (int symTableIndex = 0; symTableIndex < c1.DSExecutable.runtimeSymbols.Length; symTableIndex++)
            {
                foreach (SymbolNode symNode in c1.DSExecutable.runtimeSymbols[symTableIndex].symbolList.Values)
                {
                    ProtoCore.Mirror.RuntimeMirror langMirror = null;
                   

                    ProtoCore.Mirror.RuntimeMirror dynamoMirror = null;
                    bool lookupOk = false;


                    if (CoreUtils.IsCompilerGenerated(symNode.name) || symNode.functionIndex != Constants.kInvalidIndex)
                    {
                        continue; //Don't care about internal variables
                    }

                    if (symNode.functionIndex == Constants.kGlobalScope && symNode.classScope == Constants.kInvalidIndex)
                    {
                       
                            
                            langMirror = new ProtoCore.Mirror.RuntimeMirror(symNode.name, 0, c1);
                            string name = symNode.name + "_" + guid.ToString();
                            name = name.Replace("-", string.Empty);
                            dynamoMirror = ViewModel.Model.EngineController.GetMirror(name);
                            if (langMirror != null || dynamoMirror != null)
                            {
                                lookupOk = true;
                            }

                            if (lookupOk)
                            {
                                if (!langMirror.GetData().Equals(dynamoMirror.GetData()))
                                {
                                    Assert.Fail(string.Format("\tThe value of variable \"{0}\" doesn't match in language mode and in Dynamo CBN mode.\n", symNode.name));
                                }

                            }
                           
                    }
                }
            }

        }

        [Test]
        public void TestCBNEngineTests()
        {

            String code =
            @"
	        
               [Associative]
                {
	                foo=5;
                }

            
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            ProtoCore.Core core = thisTest.GetTestCore();
            Guid guid = RunDSScriptInCBN(code);
            ProtoCore.Core dynamoCore = ViewModel.Model.EngineController.LiveRunnerCore;
            CompareCores(core, dynamoCore,guid);
        }
        
    }
    }

