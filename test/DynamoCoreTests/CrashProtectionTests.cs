using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class CrashProtectionTests : DynamoModelTestBase
    {
        private readonly string crashProtDir = @"core\crashProtection";

        #region Helpers

        private void AssertManual(HomeWorkspaceModel ws)
        {
            Assert.AreEqual(RunType.Manual, ws.RunSettings.RunType);
        }

        private void AssertAuto(HomeWorkspaceModel ws)
        {
            Assert.AreEqual(RunType.Automatic, ws.RunSettings.RunType);
        }

        #endregion

        [Test]
        public void RunAutoFileWithoutFlagOpensInManual()
        {
            //For files what have never been opened, HasRunWithoutCrash defaults to false 
            //    -> So, all non-marked files will open in manual even if run auto is on

            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runAutoNoCrashFlag.dyn");

            AssertManual(ws);
        }

        [Test]
        public void RunAutoFileWithTrueFlagOpensInAuto()
        {
            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runAutoTrueCrashFlag.dyn");

            AssertAuto(ws);
        }


        [Test]
        public void RunAutoFileWithFalseFlagOpensInManual()
        {
            //On open file, if run auto & HasRunWithoutCrash = false, set to run manual
            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runAutoFalseCrashFlag.dyn");

            AssertManual(ws);
        }

        [Test]
        public void RunAutoFileWithSuccessfulRunSavesFlag()
        {
            //On save, if run auto & HasRunWithoutCrash = true, this should be saved
            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runAutoFalseCrashFlag.dyn");

            // We do a run so HasRunWithoutCrash is set to true.  Otherwise, the test
            // assertion is not valid.
            BeginRun();
            EmptyScheduler();

            Assert.True(ws.HasRunWithoutCrash);

            // set to auto, we expect this to be maintained in the re-opened file
            ws.RunSettings.RunType = RunType.Automatic;

            // save the file to a temp location
            var tp = Path.Combine(TempFolder, "tempCrashProtection.dyn");
            ws.SaveAs(tp, CurrentDynamoModel.EngineController.LiveRunnerCore);

            // open the file
            var nws = Open<HomeWorkspaceModel>(tp);

            AssertAuto(nws);
        }

        [Test]
        public void FlagIsFalseWhenEvaluationStarted()
        {
            //On run start, HasRunWithoutCrash = false
            //    - This makes sure that if the user has modifies the file during a run that causes a crash, there 
            //        file is not erroneously marked as having run without crash. 
            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runManual.dyn");

            // We do a run so HasRunWithoutCrash is set to true.  Otherwise, the test
            // assertion is not valid.
            BeginRun();
            EmptyScheduler();

            Assert.True(ws.HasRunWithoutCrash);

            // Update the number input to ensure another run takes place
            var n = ws.Nodes.OfType<Nodes.DoubleInput>().First();
            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(n.GUID, "Value", "3.0"));

            // We'll need this flag to ensure the run is completed even when
            // done async
            var assertionComplete = false;
            ws.EvaluationStarted += (e, a) =>
            {
                Assert.False(ws.HasRunWithoutCrash);
                assertionComplete = true;
            };

            // Do the next run
            BeginRun();
            EmptyScheduler();

            if (!assertionComplete)
            {
                Assert.Fail("The assertion was not evaluated!");
            }
        }

        [Test]
        public void FlagIsSetToTrueAfterRunSuccessfullyCompletes()
        {
            //On run complete, HasRunWithoutCrash = true
            var ws = Open<HomeWorkspaceModel>(TestDirectory, crashProtDir, "runManual.dyn");

            Assert.False(ws.HasRunWithoutCrash);

            // We'll need this flag to ensure the run is completed even when
            // done async
            var assertionComplete = false;
            ws.EvaluationCompleted += (e, a) =>
            {
                Assert.True(ws.HasRunWithoutCrash);
                assertionComplete = true;
            };

            BeginRun();
            EmptyScheduler();

            if (!assertionComplete)
            {
                Assert.Fail("The assertion was not evaluated!");
            }
        }

    }
}
