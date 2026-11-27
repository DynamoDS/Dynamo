using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class SandboxWpfPreviewSmokeTests
    {
        private const double Tolerance = 1e-4;
        private static readonly Guid DifferenceNodeId = Guid.Parse("99999999-9999-4999-8999-999999999901");

        private static string RepoRoot =>
            Environment.GetEnvironmentVariable("OPEN_GEOMETRY_REPO_ROOT")
            ?? Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", ".."));

        private static string StagingDir =>
            Environment.GetEnvironmentVariable("OPEN_GEOMETRY_STAGING_DIR")
            ?? Path.Combine(RepoRoot, "bin", "open-geometry", "sandbox", "Release");

        [Test, Apartment(ApartmentState.STA)]
        public void WhenGraphRunsWithHelixWatch3DThenPreviewMeshHasOpenBore()
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "OpenGeometryWpfSmoke", Guid.NewGuid().ToString("N"));
            var userData = Path.Combine(tempRoot, "UserData");
            var commonData = Path.Combine(tempRoot, "CommonData");
            Directory.CreateDirectory(userData);
            Directory.CreateDirectory(commonData);

            DynamoModel model = null;
            DynamoViewModel viewModel = null;
            DynamoView view = null;
            try
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
                var args = new StartupUtils.CommandLineArguments
                {
                    NoNetworkMode = true,
                    DisableAnalytics = true,
                    UserDataFolder = userData,
                    CommonDataFolder = commonData,
                    Locale = "en-US"
                };

                model = StartupUtils.MakeCLIModel(args);
                model.PreferenceSettings.SetIsBackgroundPreviewActive("IsBackgroundPreviewActive", true);

                var watch3D = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                    null,
                    new Watch3DViewModelStartupParams(model),
                    model.Logger);

                Assert.That(watch3D, Is.InstanceOf<HelixWatch3DViewModel>(),
                    "HelixWatch3DViewModel should initialize cleanly under OpenGeometry.");

                viewModel = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration
                {
                    DynamoModel = model,
                    Watch3DViewModel = watch3D,
                    ShowLogin = false
                });

                view = new DynamoView(viewModel);
                view.Show();
                watch3D.Active = true;
                PumpDispatcher(TimeSpan.FromMilliseconds(300));

                var fixturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "CenteredCuboidCylinderCut.dyn");
                model.OpenFileFromPath(fixturePath, forceManualExecutionMode: false);
                var workspace = model.CurrentWorkspace as HomeWorkspaceModel;
                Assert.That(workspace, Is.Not.Null);
                workspace.Run();

                var diffNode = workspace.Nodes.Single(n => n.GUID == DifferenceNodeId);
                var helixFactory = new HelixRenderPackageFactory();
                var scheduled = diffNode.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, helixFactory, forceUpdate: true);
                Assert.That(scheduled, Is.True);

                PumpDispatcher(TimeSpan.FromMilliseconds(1000));

                var helixVm = (HelixWatch3DViewModel)watch3D;
                var meshes = helixVm.SceneItems
                    .OfType<DynamoGeometryModel3D>()
                    .Select(m => m.Geometry as MeshGeometry3D)
                    .Where(g => g != null && g.Positions != null && g.Positions.Count > 0 && g.Indices != null && g.Indices.Count > 0)
                    .ToArray();

                Assert.That(meshes, Is.Not.Empty, "HelixWatch3DViewModel should contain rendered mesh geometry for Solid.Difference.");

                var triangles = new List<(SharpDX.Vector3 A, SharpDX.Vector3 B, SharpDX.Vector3 C)>();
                foreach (var mesh in meshes)
                {
                    for (int i = 0; i + 2 < mesh.Indices.Count; i += 3)
                    {
                        triangles.Add((
                            mesh.Positions[mesh.Indices[i]],
                            mesh.Positions[mesh.Indices[i + 1]],
                            mesh.Positions[mesh.Indices[i + 2]]));
                    }
                }

                // Helix uses Y-up: Dynamo (x, y, z) maps to Helix (X = x, Y = z, Z = -y).
                bool hasCylinderWall = triangles.Any(t =>
                    OnBoreRadius(t.A) && OnBoreRadius(t.B) && OnBoreRadius(t.C));
                bool coversBoreCenter = triangles.Any(CoversHelixBoreCenter);

                Assert.Multiple(() =>
                {
                    Assert.That(triangles.Count, Is.GreaterThan(0));
                    Assert.That(hasCylinderWall, Is.True, "Helix mesh should include the cylindrical bore wall at X^2 + Z^2 = 4.");
                    Assert.That(coversBoreCenter, Is.False, "Helix top/bottom faces must leave the through-hole open.");
                });
            }
            finally
            {
                view?.Close();
                PumpDispatcher(TimeSpan.FromMilliseconds(200));
                model?.ShutDown(false);
                if (Directory.Exists(tempRoot))
                {
                    try { Directory.Delete(tempRoot, true); }
                    catch (IOException) { }
                }
            }
        }

        [Test]
        public void WhenStagedDynamoSandboxLaunchesThenForkProtoGeometryAndOpenKernelLoadWithoutAsmOrLibG()
        {
            var sandboxExe = Path.Combine(StagingDir, "DynamoSandbox.exe");
            Assert.That(File.Exists(sandboxExe), Is.True, $"Missing staged DynamoSandbox.exe at {sandboxExe}");

            var fixturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "CenteredCuboidCylinderCut.dyn");
            var tempRoot = Path.Combine(Path.GetTempPath(), "OpenGeometrySandboxProc", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
            var commandFilePath = Path.Combine(tempRoot, "smoke.xml");
            File.WriteAllText(commandFilePath, $"""
                <Commands ExitAfterPlayback="false" PauseAfterPlaybackInMs="500" CommandIntervalInMs="200">
                  <OpenFileCommand XmlFilePath="{fixturePath}" />
                  <RunCancelCommand ShowErrors="false" CancelRun="false" />
                </Commands>
                """);

            var psi = new ProcessStartInfo
            {
                FileName = sandboxExe,
                Arguments = $"--NoNetworkMode --DisableAnalytics -c \"{commandFilePath}\"",
                WorkingDirectory = StagingDir,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            Assert.That(process, Is.Not.Null, "Failed to start staged DynamoSandbox.exe.");

            try
            {
                string[] loadedModules = Array.Empty<string>();
                string[] loadedPaths = Array.Empty<string>();
                var deadline = Stopwatch.StartNew();
                bool kernelLoaded = false;

                while (deadline.Elapsed < TimeSpan.FromSeconds(45))
                {
                    if (process.HasExited)
                    {
                        Assert.Fail($"DynamoSandbox.exe exited prematurely with code {process.ExitCode}.");
                    }

                    try
                    {
                        process.Refresh();
                        var mods = process.Modules.Cast<ProcessModule>().ToArray();
                        loadedModules = mods.Select(m => m.ModuleName).ToArray();
                        loadedPaths = mods.Select(m => m.FileName).ToArray();

                        if (loadedModules.Contains("OpenGeometryBridge.dll", StringComparer.OrdinalIgnoreCase)
                            && loadedModules.Contains("SM_API.dll", StringComparer.OrdinalIgnoreCase)
                            && loadedModules.Contains("SMLib.dll", StringComparer.OrdinalIgnoreCase)
                            && loadedModules.Contains("ProtoGeometry.dll", StringComparer.OrdinalIgnoreCase))
                        {
                            kernelLoaded = true;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Process may still be initializing PEB/modules.
                    }

                    Thread.Sleep(250);
                }

                Assert.That(kernelLoaded, Is.True,
                    "Timed out waiting for DynamoSandbox.exe to load ProtoGeometry.dll, OpenGeometryBridge.dll, SM_API.dll, and SMLib.dll. " +
                    $"Loaded modules sample: {string.Join(", ", loadedModules.Where(m => m.Contains("Dynamo", StringComparison.OrdinalIgnoreCase) || m.Contains("Proto", StringComparison.OrdinalIgnoreCase) || m.Contains("Geometry", StringComparison.OrdinalIgnoreCase) || m.Contains("SM", StringComparison.OrdinalIgnoreCase)))}");

                var expectedProtoPath = Path.Combine(StagingDir, "ProtoGeometry.dll");
                var expectedBridgePath = Path.Combine(StagingDir, "OpenGeometryBridge.dll");
                var expectedSmApiPath = Path.Combine(StagingDir, "SM_API.dll");
                var expectedSmLibPath = Path.Combine(StagingDir, "SMLib.dll");

                var loadedProtoPaths = loadedPaths
                    .Where(p => string.Equals(Path.GetFileName(p), "ProtoGeometry.dll", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                var forbiddenModules = loadedModules
                    .Where(m => m.StartsWith("ASM", StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith("LibG", StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith("DynamoShapeManager", StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith("usd_", StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith("_omni_solid", StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith("tbb", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                Assert.Multiple(() =>
                {
                    Assert.That(loadedModules, Does.Contain("DynamoCore.dll").IgnoreCase);
                    Assert.That(loadedModules, Does.Contain("DynamoCoreWpf.dll").IgnoreCase);
                    Assert.That(loadedProtoPaths, Is.EqualTo(new[] { expectedProtoPath }).IgnoreCase,
                        "Sandbox must load only the staged fork ProtoGeometry.dll.");
                    Assert.That(loadedPaths, Does.Contain(expectedBridgePath).IgnoreCase);
                    Assert.That(loadedPaths, Does.Contain(expectedSmApiPath).IgnoreCase);
                    Assert.That(loadedPaths, Does.Contain(expectedSmLibPath).IgnoreCase);
                    Assert.That(forbiddenModules, Is.Empty, "Sandbox process must not load ASM, LibG, DynamoShapeManager, OpenUSD, or TBB.");
                });
            }
            finally
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill(entireProcessTree: true);
                            process.WaitForExit(5000);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                }

                if (Directory.Exists(tempRoot))
                {
                    try { Directory.Delete(tempRoot, true); }
                    catch (IOException) { }
                }
            }
        }

        private static bool OnBoreRadius(SharpDX.Vector3 p)
        {
            return Math.Abs(p.X * p.X + p.Z * p.Z - 4.0) < 0.02;
        }

        private static bool CoversHelixBoreCenter((SharpDX.Vector3 A, SharpDX.Vector3 B, SharpDX.Vector3 C) t)
        {
            if (Math.Abs(t.A.Y - t.B.Y) > Tolerance || Math.Abs(t.A.Y - t.C.Y) > Tolerance)
                return false;

            double a = t.A.X * t.B.Z - t.A.Z * t.B.X;
            double b = t.B.X * t.C.Z - t.B.Z * t.C.X;
            double c = t.C.X * t.A.Z - t.C.Z * t.A.X;
            return (a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0);
        }

        private static void PumpDispatcher(TimeSpan duration)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < duration)
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Thread.Sleep(25);
            }
        }
    }
}
