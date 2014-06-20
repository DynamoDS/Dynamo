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
   
    public class CBNEngineTests : DSEvaluationUnitTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        
        public Guid RunScriptDynamo(string code)
        {

            var model = Controller.DynamoModel;
            var guid = Guid.NewGuid();
            var command1 = new Dynamo.ViewModels.DynamoViewModel.CreateNodeCommand(guid, "Code Block", 0, 0, false, false);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command1);

            var command2 = new Dynamo.ViewModels.DynamoViewModel.UpdateModelValueCommand(guid, "Code", code);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command2);

            var cbn = model.Nodes[0] as Dynamo.Nodes.CodeBlockNodeModel;

            Assert.DoesNotThrow(() => Controller.RunExpression(null));
            return guid;
        }


        internal  void CompareCores(ProtoCore.Core c1, ProtoCore.Core c2,Guid guid)
        {
        


            for (int symTableIndex = 0; symTableIndex < c1.DSExecutable.runtimeSymbols.Length; symTableIndex++)
            {
                foreach (SymbolNode symNode in c1.DSExecutable.runtimeSymbols[symTableIndex].symbolList.Values)
                {
                    ProtoCore.Mirror.RuntimeMirror langMirror = null;
                   

                    ProtoCore.Mirror.RuntimeMirror dynamoMirror = null;
                    bool lookupOk = false;
                    
        
                    if (symNode.name.StartsWith("%") || symNode.functionIndex != Constants.kInvalidIndex)
                    {
                        continue; //Don't care about internal variables
                    }

                    if (symNode.functionIndex == Constants.kGlobalScope && symNode.classScope == Constants.kInvalidIndex)
                    {
                        try
                        {
                            
                            langMirror = new ProtoCore.Mirror.RuntimeMirror(symNode.name, 0, c1);
                            string name = symNode.name + "_" + guid.ToString();
                            name = name.Replace("-", string.Empty);
                            dynamoMirror = Controller.EngineController.GetMirror(name);
                            lookupOk = true;
                        }
                        catch (NotImplementedException)
                        {

                        }
                        catch (Exception ex)
                        {
                            if ((ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException) &&
                                (c1.RunningBlock != symNode.runtimeTableIndex))
                            {
                                // Quite possible that variables defined in the inner
                                // language block have been garbage collected and 
                                // stack frame pointer has been adjusted when return
                                // to the outer block. 
                            }
                            else
                            {
                                throw ex;
                            }
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
        public void TestAssignment01()
        {

            String code =
            @"
	        
               [Associative]
                {
	                foo=5;
                }

            
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            ProtoCore.Core core = thisTest.getTestCore();
            Guid guid = RunScriptDynamo(code);
            ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
            CompareCores(core, dynamoCore,guid);
        }
    }
    }

