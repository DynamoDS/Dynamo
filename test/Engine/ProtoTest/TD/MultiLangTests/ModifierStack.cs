using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;

namespace ProtoTest
{
    [TestFixture]
    public class TestModifierStack
    {
        private ProtoCore.Core core;

        readonly TestFrameWork thisTest = new TestFrameWork();
        ProtoScript.Config.RunConfiguration runnerConfig;
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Update\\";  
        [SetUp]
        public void Setup() 
        {
            core = new ProtoCore.Core(new ProtoCore.Options());

            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }

       
        [Test]
        [Category("SmokeTest")]
        public void T50modifierstack_update()
        {
            String code = @"
            a = {
            { 1, 2, 3 } => a1;
            2 => a1;
            }
            a1 = 5; 
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 5);
            thisTest.Verify("a1", 5);
        }
        [Test]
        [Category("SmokeTest")]
        public void T51modify_anitem_inarray()
        {
            String code = @"
            a = {
                { 1, 2, 3 } => a1;
                  2 => a1;
                { a1 + 3, 1 } => a2;
            }
            a1 = 5;
            c = a2[0];
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 5);
            thisTest.Verify("a2", new object [] {8,1});
            thisTest.Verify("a", new object[] { 8, 1 });
            thisTest.Verify("c", 8);

        }
        [Test]
        [Category("SmokeTest")]
        public void T52_modify_anitem_inarray()
        {
            String code = @"
            a = {
                { 1, 2, 3 } => a1;
                  2 => a1;
                  a1 + 3 => a2;
            }
            a1 = 5;
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 5);
            thisTest.Verify("a2", 8);
            thisTest.Verify("a", 8);

        }
        [Test]
        [Category("SmokeTest")]
        public void T53_reassign_modifierstack()
        {
            String code = @"
            a = {
            { 1, 2, 3 } => a1;
            2 => a2;
            }
            b = {
            1 => a1;
            {2,3,4} => a3;
            }
            c = a;
            a = b;
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 2);
            thisTest.Verify("a", new object[]  { 2, 3, 4 });
            thisTest.Verify("a3", new object[] { 2, 3, 4 });
            thisTest.Verify("b", new object[]  { 2, 3 ,4 });
            thisTest.Verify("c", new object[]  { 2, 3, 4 });

        }
        [Test]
        [Category("SmokeTest")]
        public void T54_reassign_modifierstack()
        {
            String code = @"
            a = {
            { 1, 2, 3 } => a1;
            2 => a2;
            }
            b = {
            1 => a1;
            {2,3,4} => a3;
            }
            c = a;
            a = b;
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 2);
            thisTest.Verify("a", new object[] { 2, 3, 4 });
            thisTest.Verify("a3", new object[] { 2, 3, 4 });
            thisTest.Verify("b", new object[] { 2, 3, 4 });
            thisTest.Verify("c", new object[] { 2, 3, 4 });

        }
        [Test]
        [Category("SmokeTest")]
        public void T55_use_modifierstackvariable_in_imperative()
        {
            String code = @"
            a = {
                { 1, 2, 3 } => a1;
                  2 => a1;
                  a1 + 3 => a2;
            }
            d = 1;
            b=[Imperative]
            {
                c = 5;
                [Associative]
                {
	            d = {
	                { 1, 2, 3 } => a1;
	                  2 => a1;
	                  a1 + 3 => a2;
                }	
                }
                return = a1 + 5;
            }
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", 5);
            thisTest.Verify("a", 5);
            thisTest.Verify("d", 5);
            thisTest.Verify("b", 7);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T56_redefinevariable_inmodifierblock()
        {
            String code = @"
            a = {
                { 1, 2, 3 } => a1;
                  2 => a1;
                { a1 + 3, 1 } => a2;
            }
            //a1 = 5;
            c = {
                  1 => c;
	              1 => a2;
            //	  2 => d;
            };

            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", 1);
            thisTest.Verify("a", 1);
            thisTest.Verify("c", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T57_samevariable_inmodifierblock()
        {
            String code = @"
            a = {
                { 1, 2, 3 } => a1;
                  2 => a1;
                { a1 + 3, 1 } => a2;
            }
            c = {
                1 => c;
                2 => a2;
            };
            d=a2;
            ";

            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 2);
            thisTest.Verify("d", 2);
            thisTest.Verify("a", 2);
            thisTest.Verify("c", 2);
        }
      
      
    }
}

