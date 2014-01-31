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
            return theInstance_;
        }
        public int ID
        {
            get
            {
                return this.id_;
            }
            set
            {
                this.id_ = value;
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
            id_ = id;
        }
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private static Foo theInstance_ = new Foo(100);
        private int id_;
    }
    class PropertyChangedNotifyTest
    {
        private ProtoCore.Core core_;
        private DebugRunner runner_;
        private ProtoScript.Config.RunConfiguration runconfig_;
        [SetUp]
        public void Setup()
        {
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            core_ = new ProtoCore.Core(options);
            core_.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core_));
            core_.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core_));
            runconfig_ = new ProtoScript.Config.RunConfiguration();
            runconfig_.IsParrallel = false;
            runner_ = new DebugRunner(core_);
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
            runner_.PreStart(code, runconfig_);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms = runner_.StepOver();
            vms = runner_.StepOver();
            vms = runner_.StepOver();
            Obj val = GetWatchValue(core_, @"id");
            Assert.IsTrue((Int64)val.Payload == 101);
            // As Foo implements INotifyPropertyChanged interface, the property
            // change notification should be propagated back to DS virtual
            // machine so that all DS objects that depend on that property 
            // should be updated. 
            fooSingleton.ID = 202;
            // So it is expected to reexecute "id = foo.ID", that is line 3. 
            vms = runner_.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            // Expect 'id' has been updated to 202
            vms = runner_.StepOver();
            val = GetWatchValue(core_, @"id");
            Assert.IsTrue((Int64)val.Payload == 202);
        }

        [Test]
        public void RunPropertyChangedInOtherScopeTest()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");def ding(){    return = null;}foo = Foo.GetInstance();              id = foo.ID;                           r = ding();t = 1;                                ";
            runner_.PreStart(code, runconfig_);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms;
            vms = runner_.StepOver();
            vms = runner_.StepOver(); // foo = Foo.GetInstance();
            vms = runner_.StepOver(); // id = foo.ID;
            Obj val = GetWatchValue(core_, @"id");
            Assert.IsTrue((Int64)val.Payload == 101);
            vms = runner_.Step();     // return = null;
            fooSingleton.ID = 202;
            vms = runner_.Step();     // }
            vms = runner_.Step();
            // expect to re-execute id = foo.ID
            vms = runner_.StepOver();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            // Expect 'id' has been updated to 202
            vms = runner_.StepOver();
            val = GetWatchValue(core_, @"id");
            Assert.IsTrue((Int64)val.Payload == 202);
        }

        [Test]
        public void RunPropertyChangedForSameObjectTest()
        {
            string code =
@"import (Foo from ""ProtoTest.dll"");foo = Foo.GetInstance();              bar = foo;id1 = foo.ID;                           id2 = bar.ID;t = 1;                         ";
            runner_.PreStart(code, runconfig_);
            Foo fooSingleton = Foo.GetInstance();
            fooSingleton.ID = 101;
            DebugRunner.VMState vms;
            vms = runner_.StepOver(); // import ...
            vms = runner_.StepOver(); // foo = Foo.GetInstance();
            vms = runner_.StepOver(); // bar = foo;
            vms = runner_.StepOver(); // id1 = foo.ID;
            vms = runner_.StepOver(); // id2 = bar.ID;
            Obj val;
            val = GetWatchValue(core_, @"id1");
            Assert.IsTrue((Int64)val.Payload == 101);
            val = GetWatchValue(core_, @"id2");
            Assert.IsTrue((Int64)val.Payload == 101);
            fooSingleton.ID = 202;
            // expect to re-execute id1 = foo.ID
            vms = runner_.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = runner_.StepOver();
            vms = runner_.StepOver();
            val = GetWatchValue(core_, @"id1");
            Assert.IsTrue((Int64)val.Payload == 202);
            // expect to re-execute id2 = bar.ID
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = runner_.StepOver();
            val = GetWatchValue(core_, @"id2");
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
        public void RunDSPropertyChangedTest()
        {
            string code =
@"class Foo{    x;}f = Foo();f.x = 41;";
            runner_.PreStart(code, runconfig_);
            PropertyChangedVerifier v = new PropertyChangedVerifier();
            // ProtoFFI.FFIPropertyChangedMonitor.GetInstance().RegisterDSPropertyChangedHandler("f", "x", v.DSPropertyChanged);

            DebugRunner.VMState vms;
            vms = runner_.StepOver();
            vms = runner_.StepOver();
            vms = runner_.StepOver();
            Assert.True(v.IsNotified);
        }
    }
}
