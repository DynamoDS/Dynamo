using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTestFx.TD;
namespace ProtoTest.EventTests
{
    public class Foo : INotifyPropertyChanged
    {
        public static Foo GetInstance()
        {
            return theInstance;
        }
        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
                NotifyPropertyChanged("ID");
            }

        }
        public static void SetID(Foo foo, int id)
        {
            foo.ID = id;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private Foo(int id = 0)
        {
            id = id;
        }
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private static Foo theInstance = new Foo(100);
        private int id;
    }
    class PropertyChangedNotifyTest : ProtoTestBase
    {
        private DebugRunner runner;
        private ProtoScript.Config.RunConfiguration runconfig;

        public override void Setup()
        {
            base.Setup();
            runconfig = new ProtoScript.Config.RunConfiguration();
            runconfig.IsParrallel = false;
            runner = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }
        private Obj GetWatchValue(ProtoCore.Core core, string watchExpression)
        {
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core);
            ExecutionMirror mirror = watchRunner.Execute(watchExpression);
            return mirror.GetWatchValue();
        }

        [Test]
        public void RunPropertyChangedTest()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");foo = Foo.GetInstance();              id = foo.ID;                           t = 1;                                ";
            runner.PreStart(code, runconfig);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms = runner.StepOver();
            vms = runner.StepOver();
            vms = runner.StepOver();
            Obj val = GetWatchValue(core, @"id");
            Assert.IsTrue((Int64)val.Payload == 101);
            // As Foo implements INotifyPropertyChanged interface, the property
            // change notification should be propagated back to DS virtual
            // machine so that all DS objects that depend on that property 
            // should be updated. 
            fooSingleton.ID = 202;
            // So it is expected to reexecute "id = foo.ID", that is line 3. 
            vms = runner.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            // Expect 'id' has been updated to 202
            vms = runner.StepOver();
            val = GetWatchValue(core, @"id");
            Assert.IsTrue((Int64)val.Payload == 202);
        }

        [Test]
        public void RunPropertyChangedInOtherScopeTest()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");def ding(){    return = null;}foo = Foo.GetInstance();              id = foo.ID;                           r = ding();t = 1;                                ";
            runner.PreStart(code, runconfig);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms;
            vms = runner.StepOver();
            vms = runner.StepOver(); // foo = Foo.GetInstance();
            vms = runner.StepOver(); // id = foo.ID;
            Obj val = GetWatchValue(core, @"id");
            Assert.IsTrue((Int64)val.Payload == 101);
            vms = runner.Step();     // return = null;
            fooSingleton.ID = 202;
            vms = runner.Step();     // }
            vms = runner.Step();
            // expect to re-execute id = foo.ID
            vms = runner.StepOver();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            // Expect 'id' has been updated to 202
            vms = runner.StepOver();
            val = GetWatchValue(core, @"id");
            Assert.IsTrue((Int64)val.Payload == 202);
        }

        [Test]
        public void RunPropertyChangedForSameObjectTest()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");foo = Foo.GetInstance();              bar = foo;id1 = foo.ID;                           id2 = bar.ID;t = 1;                         ";
            runner.PreStart(code, runconfig);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms;
            vms = runner.StepOver(); // import ...
            vms = runner.StepOver(); // foo = Foo.GetInstance();
            vms = runner.StepOver(); // bar = foo;
            vms = runner.StepOver(); // id1 = foo.ID;
            vms = runner.StepOver(); // id2 = bar.ID;
            Obj val;
            val = GetWatchValue(core, @"id1");
            Assert.IsTrue((Int64)val.Payload == 101);
            val = GetWatchValue(core, @"id2");
            Assert.IsTrue((Int64)val.Payload == 101);
            fooSingleton.ID = 202;
            // expect to re-execute id1 = foo.ID
            vms = runner.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = runner.StepOver();
            vms = runner.StepOver();
            val = GetWatchValue(core, @"id1");
            Assert.IsTrue((Int64)val.Payload == 202);
            // expect to re-execute id2 = bar.ID
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = runner.StepOver();
            val = GetWatchValue(core, @"id2");
            Assert.IsTrue((Int64)val.Payload == 202);
        }

        [Test]
        public void RunPropertyChangedForRunMode()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");foo = Foo.GetInstance();              foo.ID = 17;id = foo.ID;Foo.SetID(foo, 41);               ";
            var testRunner = new TestFrameWork();
            testRunner.RunScriptSource(code);
            testRunner.Verify("id", 41);
        }

        [Test]
        public void RunPropertyChangedNegative()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");foo = Foo.GetInstance();              foo.ID = 17;id = foo.ID;id = bar.ID;        // RedefinitionFoo.SetID(foo, 41);               ";
            var testRunner = new TestFrameWork();
            testRunner.RunScriptSource(code);
            testRunner.Verify("id", null);
        }
        class PropertyChangedVerifier
        {
            public PropertyChangedVerifier()
            {
                IsNotified = false;
            }
            public bool IsNotified
            {
                get;
                set;
            }
            public void DSPropertyChanged(ProtoFFI.DSPropertyChangedEventArgs args)
            {
                IsNotified = true;
            }
        }

        [Test]
        [NUnit.Framework.Category("Failure")]
        public void RunDSPropertyChangedTest()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4391
            string code =
@"class Foo{    x;}f = Foo();f.x = 41;";
            runner.PreStart(code, runconfig);
            PropertyChangedVerifier v = new PropertyChangedVerifier();

            DebugRunner.VMState vms;
            vms = runner.StepOver();
            vms = runner.StepOver();
            vms = runner.StepOver();
            string err = "MAGN-4391: Failed to track property change";
            Assert.True(v.IsNotified, err);
        }
    }
}
