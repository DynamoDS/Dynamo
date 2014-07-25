       
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
public class ChangeMe : CBNEngineTests
{
public TestFrameWork thisTest = new TestFrameWork();
 [Test]
[Category("LanguageCBNTest")]
public void BIM01_SomeNulls()
     {
     string code =
          @"
     
      
      a = {null,20,30,null,{10,0},0,5,2};
      b = {1,2,3};
      e = {3,20,30,4,{null,0},0,5,2};
      c = SomeNulls(a);
      d = SomeNulls(b);
      f = SomeNulls(e);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM02_CountTrue()
     {
     string code =
          @"
     
      a = {true,true,true,false,{true,false},true,{false,false,{true,{false},true,true,false}}};
      b = {true,true,true,false,true,true};
      c = {true,true,true,true,true,true,true};
      w = CountTrue(a);
      x = CountTrue(b);
      y = CountTrue(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM03_CountFalse()
     {
     string code =
          @"
     
      a = {true,true,true,false,{true,false},true,{false,false,{true,{false},true,true,false}}};
      b = {true,true,true,false,true,true};
      c = {true,true,true,true,true,true,true};
      e = CountFalse(a);
      f = CountFalse(b);
      g = CountFalse(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM04_AllFalse_AllTrue()
     {
     string code =
          @"
     
      
      a = {true};
      b = {false,false,{false,{false,{false,false,{false},false}}},false};
      c = {true,true,true,true,{true,true},true,{true,true,{true, true,{true},true,true,true}}};
      d = AllTrue(a);
      e = AllTrue(b);
      f = AllTrue(c);
      g = AllFalse(a);
      h = AllFalse(b);
      i = AllFalse(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM05_IsHomogeneous()
     {
     string code =
          @"
     
      a = {1,2,3,4,5};
      b = {false, true, false};
      c = {{1},{1.0,2.0}};
      d = {null,1,2,3};
      e = {};
      ca = IsHomogeneous(a);
      cb = IsHomogeneous(b);
      cc = IsHomogeneous(c);
      cd = IsHomogeneous(d);
      ce = IsHomogeneous(e);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM06_SumAverage()
     {
     string code =
          @"
     
      
      b = {1,2,{3,4,{5,{6,{7},8,{9,10},11}},12,13,14,{15}},16};
      c = {1.2,2.2,{3.2,4.2,{5.2,{6.2,{7.2},8.2,{9.2,10.2},11.2}},12.2,13.2,14.2,{15.2}},16.2};
      x = Average(b);
      y = Sum(b);
      z = Average(c);
      s = Sum(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM07_SomeTrue_SomeFalse()
     {
     string code =
          @"
     
      a = {true,true,true,{false,false,{true, true,{false},true,true,false}}};
      b = {true,true,{true,true,true,{true,{true},true},true},true};
      c = {true, false, false};
      p = SomeTrue(a);
      q = SomeTrue(b);
      r = SomeTrue(c);
      s = SomeFalse(a);
      t = SomeFalse(b);
      u = SomeFalse(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM08_Remove_RemoveDuplicate()
     {
     string code =
          @"
     
      a = {null,20,30,null,20,15,true,true,5,false};
      b = {1,2,3,4,9,4,2,5,6,7,8,7,1,0,2};
      rda = RemoveDuplicates(a);
      rdb = RemoveDuplicates(b);
      ra = Remove(a,3);
      rb = Remove(b,2);
      p = rda[3];
      q = rdb[4];
      x = ra[3];
      y = rb[2];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM09_RemoveNulls()
     {
     string code =
          @"
     
      a = {1,{6,null,7,{null,null}},7,null,2};
      b = {null,{null,{null,{null},null},null},null};
      p = RemoveNulls(a);
      q = RemoveNulls(b);
      x = p[3];
      y = p[1][1];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM10_RemoveIfNot()
     {
     string code =
          @"
     
      a = {""This is "",""a very complex "",""array"",1,2.0,3,false,4.0,5,6.0,true,{2,3.1415926},null,false,'c'};
      b = RemoveIfNot(a, ""int"");
      c = RemoveIfNot(a, ""double"");
      d = RemoveIfNot(a, ""bool"");
      e = RemoveIfNot(a, ""array"");
      q = b[0];
      r = c[0];
      s = d[0];
      t = e[0][0];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM11_Reverse()
     {
     string code =
          @"
     
      a = {1,{{1},{3.1415}},null,1.0,12.3};
      b = {1,2,{3}};
      p = Reverse(a);
      q = Reverse(b);
      x = p[0];
      y = q[0][0];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM12_Contains()
     {
     string code =
          @"
     
      a = {1,{{1},{3.1415}},null,1.0,12.3};
      b = {1,2,{3}};
      x = {{1},{3.1415}};
      r = Contains(a, 3.0);
      s = Contains(a, x);
      t = Contains(a, null);
      u = Contains(b, b);
      v = Contains(b, {3});
      w = Contains(b, 3);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM13_IndexOf()
     {
     string code =
          @"
     
      a = {1,{{1},{3.1415}},null,1.0,12,3};
      b = {1,2,{3}};
      c = {1,2,{3}};
      d = {{1},{3.1415}};
      r = IndexOf(a, d);
      s = IndexOf(a, 1);
      t = IndexOf(a, null);
      u = IndexOf(b, {3});
      v = IndexOf(b, 3);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM14_Sort()
     {
     string code =
          @"
     
      a = {1,3,5,7,9,8,6,4,2,0};
      b = {1.3,2,0.8,2,null,2,2.0,2,null};
      x = Sort(a);
      x1 = Sort(a,true);
      x2 = Sort(a,false);
      y = Sort(b);
      p = x[0];
      p1 = x1[0];
      p2 = x2[0];
      q = x[9];
      s = y[0];
      t = y[7];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM15_SortIndexByValue()
     {
     string code =
          @"
     
      a = {1,3,5,7,9,8,6,4,2,0};
      b = {1.3,2,0.8,2,null,2,2.0,2,null};
      x = SortIndexByValue(a);
      x1 = SortIndexByValue(a,true);
      x2 = SortIndexByValue(a,false);
      y = SortIndexByValue(b);
      p = x[0];
      p1 = x1[0];
      p2 = x2[0];
      q = x[9];
      s = y[0];
      t = y[7];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM16_Insert()
     {
     string code =
          @"
     
      a = {false,2,3.1415926,null,{false}};
      b = 1;
      c = {1};
      d = {};
      e = {{1},2,3.0};
      p = Insert(a,b,1);
      q = Insert(a,c,1);
      r = Insert(a,d,0);
      s = Insert(a,e,5);
      u = p[1];
      v = q[1][0];
      w = r[1][0];
      x = s[5][0][0];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM17_SetDifference_SetUnion_SetIntersection()
     {
     string code =
          @"
     
      a = {false,15,6.0,15,false,null,15.0};
      b = {10,20,false,12,21,6.0,15,null,8.2};
      c = SetDifference(a,b);
      d = SetDifference(b,a);
      e = SetIntersection(a,b);
      f = SetUnion(a,b);
      p = c[0];
      q = d[1];
      r = e[1];
      s = f[1];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM18_Reorder()
     {
     string code =
          @"
     
      a = {1,4,3,8.0,2.0,0};
      b = {2,1,0,3,4};
      c = Reorder(a,b);
      p = c[0];
      q = c[1];
      r = c[2];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM19_IsUniformDepth()
     {
     string code =
          @"
     
      a = {};
      b = {1,2,3};
      c = {{1},{2,3}};
      d = {1,{2},{{3}}};
      p = IsUniformDepth(a);
      q = IsUniformDepth(b);
      r = IsUniformDepth(c);
      s = IsUniformDepth(d);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM20_NormalizeDepth()
     {
     string code =
          @"
     
      a = {{1,{2,3,4,{5}}}};
      p = NormalizeDepth(a,1);
      q = NormalizeDepth(a,2);
      r = NormalizeDepth(a,4);
      s = NormalizeDepth(a);
      w = p[0];
      x = q[0][0];
      y = r[0][0][0][0];
      z = s[0][0][0][0];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM21_Map_MapTo()
     {
     string code =
          @"
     
      a = Map(80.0, 120.0, 100.0);
      b = MapTo(0.0, 100.0 ,25.0, 80.0, 90.0);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM22_Transpose()
     {
     string code =
          @"
     
      a = {{1,2,3},{1,2},{1,2,3,4,5,6,7}};
      p = Transpose(a);
      q = Transpose(p);
      x = p[6][0];
      y = q[2][6];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM23_LoadCSV()
     {
     string code =
          @"
     
      a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set1/test1.csv"";
      b = ImportFromCSV(a);
      c = ImportFromCSV(a, false);
      d = ImportFromCSV(a, true);
      x = b[0][2];
      y = c[0][2];
      z = d[0][2];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM24_Count()
     {
     string code =
          @"
     
      a = {1, 2, 3, 4};
      b = { { 1, { 2, 3, 4, { 5 } } } };
      c = { { 2, null }, 1, ""str"", { 2, { 3, 4 } } };
      x = Count(a);
      y = Count(b);
      z = Count(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM25_Rank()
     {
     string code =
          @"
     
      a = { { 1 }, 2, 3, 4 };
      b = { ""good"", { { null } }, { 1, { 2, 3, 4, { 5, { ""good"" }, { null } } } } };
      c = { { null }, { 2, ""good"" }, 1, null, { 2, { 3, 4 } } };
      x = Rank(a);
      y = Rank(b);
      z = Rank(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM26_Flatten()
     {
     string code =
          @"
     
      a = {1, 2, 3, 4};
      b = { ""good"", { 1, { 2, 3, 4, { 5 } } } };
      c = { null, { 2, ""good""}, 1, null, { 2, { 3, 4 } } };
      q = Flatten(a);
      p = Flatten(b);
      r = Flatten(c);
      x = q[0];
      y = p[2];
      z = r[4];
      s = p[0];
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM27_Conversion_Resolution_Cases()
     {
     string code =
          @"
     
      a = {null,20,30,null,{10,0},true,{false,0,{true,{false},5,2,false}}};
      b = {1,2,{3,4,9},4,2,5,{6,7,{8}},7,1,0,2};
      x = CountTrue(a);
      y = CountFalse(a);
      z = AllTrue(a);
      w = AllFalse(a);
      p = SomeTrue(a);
      q = SomeTrue(a);
      r = Sum(true);
      s = Sum(null);
      t = RemoveDuplicates(b);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM28_IsRectangular()
     {
     string code =
          @"
     
      a = {{1,{2,3}},{4, 5, 6}};
      b= {{1, 2, 3, 4}, {5, 6, 7, 8}};
      c= {};
      x = IsRectangular(a);
      y = IsRectangular(b);
      z = IsRectangular(c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM29_RemoveIfNot()
     {
     string code =
          @"
     
      
      a = { true,null,false,true};
      b = RemoveIfNot(a, ""bool""); 
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM30_RemoveDuplicate()
     {
     string code =
          @"
     
      
      import(""FFITarget.dll"");
      pt1 = ClassFunctionality.ClassFunctionality(0, 0, 0);
      pt2 = ClassFunctionality.ClassFunctionality(0, 0, 1);
      class C
      {
      x : int;
      y : ClassFunctionality;
      constructor(p : int, q : ClassFunctionality)
      {
      x = p;
      y = q;
      }
      }
      a = {null,20,30,null,20,15,true,true,5,false};
      b = {1,2,3,4,9,4,2,5,6,7,8,7,1,0,2};
      c1 = C.C(1, pt1);
      c2 = C.C(2, pt2);
      c3 = C.C(1, pt1);
      c4 = C.C(2, pt2);
      rda = RemoveDuplicates(a);
      rdb = RemoveDuplicates(b);
      rdc = RemoveDuplicates({c1,c2,c3,c4});
      rdd = RemoveDuplicates({{1,2,{5,{6}}}, {1,2,{5,6}}, {1,2,{5,{6}}}});
      rde = RemoveDuplicates({""hello2"", ""hello"", 'r', ""hello2"", 's', 's', ""hello"", ' '});
      rdf = RemoveDuplicates({});
      rdg = RemoveDuplicates(1);
      rdh = RemoveDuplicates({{c1,c2,c3},{c3,c2,c1},{c1,c2},{c2,c3,c1},{c3,c2,c1}});
      p = rda[3];
      q = rdb[4];
      l = rdc[0];
      z = l.x;
      m1 = rdc[1].y.IntVal;
      m2 = rdd[0][2][0];
      m3 = rde[4];
      m4 = rdh[2][1].x;
      res1 = Count(rda);
      res2 = Count(rdb);
      res3 = Count(rdc);
      res4 = Count(rdd);
      res5 = Count(rde);
      res6 = Count(rdf);
      res7 = Count(rdh);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM31_Sort()
     {
     string code =
          @"
     
      a = { 3, 1, 2 };
      def sorterFunction(a : double, b : int)
      {
      return = a > b ? 1 : -1;
      }
      sort = Sort(sorterFunction, a);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM31_Sort_duplicate()
     {
     string code =
          @"
     
      c = { 3, 1, 2, 2,null };
      def sorterFunction(a : int, b : int)
      {
      return = [Imperative]
      {
      if (a == null)
      return = -1;
      if (b == null)
      return = 1;
      return = a - b;
      }
      }
      sort = Sort(sorterFunction, c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void BIM31_Sort_null()
     {
     string code =
          @"
     
      c = { 3, 1, 2,null };
      def sorterFunction(a : int, b : int)
      {
      return = [Imperative]
      {
      if (a == null)
      return = -1;
      if (b == null)
      return = 1;
      return = a > b ? 10 : -10;
      }
      }
      sort = Sort(sorterFunction, c);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
 
  [Test]
[Category("LanguageCBNTest")]
public void CBNEngineTests()
     {
     string code =
          @"
     
     	
        [Associative]
     {
     	foo=5;
     }
     
     
     
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void DoubleOp()
     {
     string code =
          @"
     
      
       
       b;
       [Associative]
       {
       	a = 1 + 2;
       b = 2.0;
       b = a + b; 
       }
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void FunctionWithinConstr001()
     {
     string code =
          @"
     
      
       
       class Dummy
       {
       		x : var;
       def init : bool ()
       {   
       			x = 5;
       			return=false;
       }
       
       constructor Create()
       {
       dummy = init();			
       }
       }
       a;
       [Associative]
       {
       d = Dummy.Create();	
       	a = d.x;
       }
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void InlineCondition001()
     {
     string code =
          @"
     
      
       
       	a = 10;
       b = 20;
       c = a < b ? a : b;
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void InlineCondition002()
     {
     string code =
          @"
     
      
       	
       	a = 10;
       			b = 20;
       c = a > b ? a : a == b ? 0 : b; 
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void InlineCondition003()
     {
     string code =
          @"
     
      
       
       a = {11,12,10};
       t = 10;
       b = a > t ? 2 : 1;
       x = b[0];
       y = b[1];
       z = b[2];
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void InlineCondition004()
     {
     string code =
          @"
     
      
       
       def f(i : int)
       {
       return = i + 1;
       }
       def g()
       {
       return = 1;
       }
       a = {10,0,10};
       t = 1;
       b = a > t ? f(10) : g();
       x = b[0];
       y = b[1];
       z = b[2];
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void LogicalOp001()
     {
     string code =
          @"
     
      
       
       e;
       [Associative]
       {
       	a = true;
       	b = false;
       c = 1;
       d = a && b;
       e = c && d;
       }
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void LogicalOp002()
     {
     string code =
          @"
     
      
       
       e;
       [Associative]
       {
       	a = true;
       	b = false;
       c = 1;
       d = a || b;
       e = c || d;
       }
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void LogicalOp003()
     {
     string code =
          @"
     
      
       
       c;
       [Associative]
       {
       	a = true;
       	b = false;
       c = !(a || !b);
       }
       
       
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer01()
     {
     string code =
          @"
     
      
      def foo(x, y, z)
      {
      return = x + y + z;
      }
      
      param = { 2, 3, 4 };
      x = Evaluate(foo, param);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer02()
     {
     string code =
          @"
     
      
      def foo(x, y, z)
      {
      return = x + y + z;
      }
      
      def foo(x, y)
      {
      return = x * y;
      }
      
      param = { 2, 3, 4 };
      x = Evaluate(foo, param);
      param = { 5, 6 };
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer03()
     {
     string code =
          @"
     
      
      def foo(x, y, z)
      {
      return = x + y + z;
      }
      
      def bar(x, y, z)
      {
      return = x * y * z;
      }
      
      t = foo;
      param = { 2, 3, 4 };
      x = Evaluate(t, param);
      t = bar;
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer04()
     {
     string code =
          @"
     
      
      def foo(x, y, z)
      {
      return = x + y + z;
      }
      
      def bar(x, y, z)
      {
      return = x * y * z;
      }
      
      param = {2, 3, 4 };
      x = Evaluate({ foo, bar }, param);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer05()
     {
     string code =
          @"
     
      
      def foo(x, y, z)
      {
      return = x + y + z;
      }
      
      param = {{2, 3, 4}, {5,6,7}, {8, 9, 10} };
      // e.q. 
      // foo({2,3,4}, {5,6,7}, {8, 9, 10});
      x = Evaluate(foo, param);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer06()
     {
     string code =
          @"
     
      
      def bar(x, y)
      {
      return = x * y;
      }
      
      def foo(f : function, x, y)
      {
      return = f(x, y);
      }
      
      x = Evaluate(foo, { bar, 2, 3 });
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer07()
     {
     string code =
          @"
     
      
      def multiplyBy2(z)
      {
      return = 2 * z;
      }
      
      def bar(y, z)
      {
      return = y * Evaluate(multiplyBy2, { z });
      }
      
      def foo(x, y, z)
      {
      return = x + Evaluate(bar, { y, z });
      }
      
      x = Evaluate(foo, { 2, 3, 4 });
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer08()
     {
     string code =
          @"
     
      
      def f1(x)
      {
      return = 2 * x;
      }
      
      def f2(x)
      {
      return = 3 * x;
      }
      
      def foo(evalFunction : function, fptr : function, param : var[])
      {
      return = evalFunction(fptr, param);
      }
      
      x = foo({ Evaluate, Evaluate }, { f1, f2 }, { { 41 }, { 42 } });
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer09()
     {
     string code =
          @"
     
      
      class Foo
      {
      fptr : function;
      params : var[]..[];
      
      constructor Foo(f : function, p: var[]..[])
      {
      fptr = f;
      params = p;
      }
      
      def DoEvaluate()
      {
      return = Evaluate(fptr, params);
      }
      }
      
      def foo(x)
      {
      return = 2 * x;
      }
      
      def constructFoo(f : function, p : var[]..[])
      {
      return = Foo(f, p);
      }
      
      x = constructFoo(foo, { 42 });
      y = x.DoEvaluate();
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
  [Test]
[Category("LanguageCBNTest")]
public void Test_EvaluateFunctionPointer10()
     {
     string code =
          @"
     
      
      def foo(x)
      {
      Print(x);
      return = [Imperative]
      {
      if (x == 0)
      {
      return=1;
      }
      else
      {
      return = x * Evaluate(foo, { x - 1 });
      }
      }
      }
      
      x = foo(5);
      
      
     ";
     ExecutionMirror mirror = thisTest.RunScriptSource(code);
     ProtoCore.Core core = thisTest.GetTestCore();
     Guid guid = this.RunDSScriptInCBN(code);
     ProtoCore.Core dynamoCore =Controller.EngineController.LiveRunnerCore;
     this.CompareCores(core, dynamoCore,guid);
     }
 }
}
 

