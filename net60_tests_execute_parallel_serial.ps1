# Requires PowerShell 7+
# Usage:
#   pwsh -File .\net60_tests_execute_parallel_serial.ps1 -TestTypeToRun all
#   pwsh -File .\net60_tests_execute_parallel_serial.ps1 -TestTypeToRun coreonly

param(
	[ValidateSet('all', 'coreonly')]
	[string]$TestTypeToRun = 'all'
)

$baseDir = "C:\Users\DeyanNenov\Documents\GitHub\Dynamo\bin\AnyCPU\Debug"

Write-Host "The test type to execute is $TestTypeToRun tests"

$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'

if ($PSVersionTable.PSVersion.Major -lt 7) {
	throw "This script requires PowerShell 7+ for ForEach-Object -Parallel."
}

# Ensure output dir
$resultsDir = Join-Path -Path (Get-Location) -ChildPath "TestResults"
if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir | Out-Null }

# shared error state
$errors = @{ "err_code" = 0 }

function ExecuteTests {
	param([string[]] $Cmds)

	if (-not $Cmds -or $Cmds.Count -eq 0) {
		Write-Host "No commands to run."
		return
	}

	$et = Measure-Command {
		$Cmds | ForEach-Object -Parallel {
			$cmd = $_
			try {
				Write-Host "RUNNING: $cmd"
				Invoke-Expression $cmd

				if ($LASTEXITCODE -ne 0) {
					Write-Host "dotnet test exited $LASTEXITCODE :"
					Write-Host $cmd
					($using:errors).err_code = $LASTEXITCODE
				}
			}
			catch {
				Write-Host "dotnet test threw:"
				Write-Host $cmd
				Write-Host $_
				($using:errors).err_code = 1
			}
		} -ThrottleLimit 10 | Out-Default
	}

	Write-Host "Elapsed time:`n$($et.ToString())"
}

# -----------------------------
# Build filters
# -----------------------------

$Tests1 = 'PythonNodeCustomizationTests',
'PreviewBubbleTests',
'NodeViewTests',
'NodeViewCustomizationTests',
'NodeAutoCompleteSearchTests',
'DocumentationBrowserViewExtensionTests'

$Tests2 = 'JSONSerializationTests.SerializationNonGuidIdsTest',
'Dynamo.Tests.WorkspaceSaving',
'AnalysisTests.LabelTests',
'Dynamo.Tests.ComplexTests',
'Dynamo.Tests.DynamoSamples',
'Dynamo.Tests.GeometryDefectTests',
'Dynamo.Tests.PackageValidationTest',
'ProtoFFITests',
'ProtoTest',
'GraphNodeManagerViewExtensionTests',
'PreferenceSettingsTests',
'Dynamo.Tests.Logging',
'Dynamo.Tests.ProfilingTest',
'DynamoCoreWpfTests.SplashScreenTests',
'Dynamo.Tests.GraphLayoutTests',
'ConnectorViewModelTests',
'PeriodicUpdateTest',
'AnnotationViewTests'

$Tests3 = 'JSONSerializationTests.SerializationTest',
'WorkspaceDependencyViewExtensionTests',
'AnnotationViewModelTests',
'CoreUserInterfaceTests',
'DocumentationBrowserViewExtensionContentTests',
'DynamoUtilitiesTests',
'CommandLineTests',
'PythonEvalTests',
'PythonEditTests',
'DynamoPythonTests',
'IntegrationTests'

$Tests4 = 'WpfVisualizationTests',
'DynamoCoreWpfTests.PackageManager',
'Dynamo.PackageManager',
'CrashReportingTests',
'RecentFileTests',
'WorkspaceOpeningTests',
'CodeBlockNodeTests',
'ColorPaletteTests',
'ConnectorContextMenuTests',
'ConverterViewModelTests',
'DynamoViewTests',
'ElementResolverTests',
'GroupStylesTests',
'LiveChartsTests',
'NoteViewTests',
'PackagePathTests',
'PreferencesGeometryScalingTests',
'RunSettingsTests',
'UnitsUITests'

$Tests5 = 'RecordedTests',
'RecordedTestsDSEngine'

$Tests6 = 'Dynamo.Tests.ListTests',
'Dynamo.Tests.MigrationTestFramework',
'Dynamo.Tests.SerializationTests'

# Serial-only
$SerialTests = 'Dynamo.Tests.PeriodicEvaluationTests.StartPeriodicEvaluation_CompletesFewerRunsWhenRunTimeIsGreaterThanEvaluationTime',
'Dynamo.Tests.SearchSideEffects.LuceneSearchTSplineNodesOrderingValidation'

function Build-FilterOrGroup([string[]] $names) {
	if (-not $names -or $names.Count -eq 0) { return $null }
	return '(FullyQualifiedName~' + ($names -join '|FullyQualifiedName~') + ')'
}

$filterTests1 = Build-FilterOrGroup $Tests1
$filterTests2 = Build-FilterOrGroup $Tests2
$filterTests3 = Build-FilterOrGroup $Tests3
$filterTests4 = Build-FilterOrGroup $Tests4
$filterTests5 = Build-FilterOrGroup $Tests5
$filterTests6 = Build-FilterOrGroup $Tests6
$filterSerialTests = Build-FilterOrGroup $SerialTests

$filterRestOfTests =
'FullyQualifiedName!~' + ($Tests1 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($Tests2 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($Tests3 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($Tests4 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($Tests5 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($Tests6 -join '&FullyQualifiedName!~') +
'&FullyQualifiedName!~' + ($SerialTests -join '&FullyQualifiedName!~')

$filterFailures = "TestCategory!=FailureNET6&TestCategory!=Failure"

# -----------------------------
# Build DLL list with proper paths
# -----------------------------

$dllNames = @('ProtoTest.dll', 'ProtoTestFx.dll')
$dllNames += (Get-ChildItem -Path $baseDir -Filter *Tests.dll -Name -File | Sort-Object -Unique)

$fullDlls = $dllNames | ForEach-Object { '"' + (Join-Path $baseDir $_) + '"' }
$TestDlls = $fullDlls -join ' '

$testCmd = "dotnet test --arch x64 -v n $TestDlls"

if ($TestTypeToRun -eq "all") {
	$testCmd1 = $testCmd + " --filter `"${filterFailures}&${filterTests1}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.1.xml`" *> test_log_1.log"
	$testCmd2 = $testCmd + " --filter `"${filterFailures}&${filterTests2}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.2.xml`" *> test_log_2.log"
	$testCmd3 = $testCmd + " --filter `"${filterFailures}&${filterTests3}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.3.xml`" *> test_log_3.log"
	$testCmd4 = $testCmd + " --filter `"${filterFailures}&${filterTests4}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.4.xml`" *> test_log_4.log"
	$testCmd5 = $testCmd + " --filter `"${filterFailures}&${filterTests5}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.5.xml`" *> test_log_5.log"
	$testCmd6 = $testCmd + " --filter `"${filterFailures}&${filterTests6}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.6.xml`" *> test_log_6.log"
	$testCmd7 = $testCmd + " --filter `"${filterFailures}&${filterRestOfTests}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.7.xml`" *> test_log_7.log"
	$testSerialCmd = $testCmd + " --filter `"${filterFailures}&${filterSerialTests}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.8.xml`" *> test_log_8.log"

	$testCmds = @(
		$testCmd1, $testCmd2, $testCmd3, $testCmd4, $testCmd5, $testCmd6, $testCmd7
	)

	Write-Host "Execute Serial tests"
	ExecuteTests @($testSerialCmd)

	Write-Host "Execute Parallel tests"
	ExecuteTests $testCmds
}
elseif ($TestTypeToRun -eq "coreonly") {
	$filterCoreFailures = "${filterFailures}&TestCategory!=FailureDynCore"

	$dlls = @('DynamoCoreTests.dll', 'DisplayTests.dll', 'AnalysisTests.dll', 'DynamoUtilitiesTests.dll', 'ProtoTest.dll')
	$fullCoreDlls = $dlls | ForEach-Object { '"' + (Join-Path $baseDir $_) + '"' }
	$coreCmd = "dotnet test $($fullCoreDlls -join ' ') --arch x64 -v q --filter `"${filterCoreFailures}`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.xml`" -- NUnit.ConsoleOut=0 *> test_log_core.log"

	Write-Host "Execute Core-only tests"
	ExecuteTests @($coreCmd)
}

if ($errors.err_code -ne 0) {
	Write-Host "One or more test batches failed. Exit code: $($errors.err_code)"
	exit $errors.err_code
}
