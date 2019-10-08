using NUnit.Framework;
namespace ProtoTest
{
    [TestFixture]
    class MultiLanugageBasic : ProtoTestBase
    {
        [Test]
        public void TestSingleLanguageImperative()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
@"
[Imperative]
{
    a = 3;
    b = 4;
}
", core);
        }
        [Test]
        public void TestSingleLanguageAssociative()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
@"
[Associative]
{
    a = 3;
    b = 4;
}
", core);
        }
        [Test]
        public void TestMultLanguageAssociativeImperative()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
@"
[Associative]
{
    a = [Imperative]
        {
            return= 5;
        }
    b = 4;
}
", core);
        }
        [Test]
        public void TestMultLanguageImperativeAssociative()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
@"
[Imperative]
{
    [Associative]
    {
        return= 5;
    }
    b = 4;
}
", core);
        }
        [Test]
        public void TestMultLanguageVariableUsage()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();

            runtimeCore = fsr.Execute(
@"
[Associative]
{
    a = 2;
    [Imperative]
    {
        if(a == 2 )
        {
            b = a + 5;
            a = 20;
        }
        else 
        {
            b = 4;
        }
    }
    c = a;
}
", core);
        }
        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestClassUsageInImpeartive()
        {
            string code =
@"
import(""FFITarget.dll"");
x;y;
i=[Imperative]
{
	p = ClassFunctionality.ClassFunctionality(16);
    x = p.IntVal;
    p.IntVal = 32;
    y = p.IntVal;
    return [x,y];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] {16, 32});
        }
    }
}
