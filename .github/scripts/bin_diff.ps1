################################################################################
### SOURCE: https://gist.github.com/cchamberlain/883959151aa1162e73f1        ###
### USAGE: bin_diff.ps1 path/to/left,path/to/right [-s path/to/summary/dir]  ###
### ADD LOCATION OF THIS SCRIPT TO PATH                                      ###
################################################################################
[CmdletBinding()]
param (
  [parameter(HelpMessage="Stores the execution working directory.")]
  [string]$ExecutionDirectory=$PWD,

  [parameter(Position=0,HelpMessage="Compare two directories recursively for differences.")]
  [alias("c")]
  [string[]]$Compare,

  [parameter(HelpMessage="Export a summary to path.")]
  [alias("s")]
  [string]$ExportSummary,

  [parameter(HelpMessage="Source of the diff job.")]
  [alias("src")]
  [string]$DiffBuildSource
)

### GLOBAL VARIABLES ###
$global:storeResult=$null;

### FUNCTION DEFINITIONS ###

# SETS WORKING DIRECTORY FOR .NET #
function SetWorkDir($PathName, $TestPath) {
  $AbsPath = NormalizePath $PathName $TestPath
  Set-Location $AbsPath
  [System.IO.Directory]::SetCurrentDirectory($AbsPath)
}

# RESTORES THE EXECUTION WORKING DIRECTORY AND EXITS #
function SafeExit() {
  SetWorkDir /path/to/execution/directory $ExecutionDirectory
  Exit
}

function Print {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$TRUE,Position=0,HelpMessage="Message to print.")]
    [string]$Message,

    [parameter(HelpMessage="Specifies a success.")]
    [alias("s")]
    [switch]$SuccessFlag,

    [parameter(HelpMessage="Specifies a warning.")]
    [alias("w")]
    [switch]$WarningFlag,

    [parameter(HelpMessage="Specifies an error.")]
    [alias("e")]
    [switch]$ErrorFlag,

    [parameter(HelpMessage="Specifies a fatal error.")]
    [alias("f")]
    [switch]$FatalFlag,

    [parameter(HelpMessage="Specifies a info message.")]
    [alias("i")]
    [switch]$InfoFlag = !$SuccessFlag -and !$WarningFlag -and !$ErrorFlag -and !$FatalFlag,

    [parameter(HelpMessage="Specifies blank lines to print before.")]
    [alias("b")]
    [int]$LinesBefore=0,

    [parameter(HelpMessage="Specifies blank lines to print after.")]
    [alias("a")]
    [int]$LinesAfter=0,

    [parameter(HelpMessage="Specifies if program should exit.")]
    [alias("x")]
    [switch]$ExitAfter
  )
  PROCESS {
    if($LinesBefore -ne 0) {
      foreach($i in 0..$LinesBefore) { Write-Host "" }
    }
    if($InfoFlag) { Write-Host "$Message" }
    if($SuccessFlag) { Write-Host "$Message" -ForegroundColor "Green" }
    if($WarningFlag) { Write-Host "$Message" -ForegroundColor "Orange" }
    if($ErrorFlag) { Write-Host "$Message" -ForegroundColor "Red" }
    if($FatalFlag) { Write-Host "$Message" -ForegroundColor "Red" -BackgroundColor "Black" }
    if($LinesAfter -ne 0) {
      foreach($i in 0..$LinesAfter) { Write-Host "" }
    }
    if($ExitAfter) { SafeExit }
  }
}

# VALIDATES STRING MIGHT BE A PATH #
function ValidatePath($PathName, $TestPath) {
  If([string]::IsNullOrWhiteSpace($TestPath)) {
    Print -x -f "$PathName is not a path"
  }
}

# NORMALIZES RELATIVE OR ABSOLUTE PATH TO ABSOLUTE PATH #
function NormalizePath($PathName, $TestPath) {
  ValidatePath "$PathName" "$TestPath"
  $TestPath = [System.IO.Path]::Combine((pwd).Path, $TestPath)
  $NormalizedPath = [System.IO.Path]::GetFullPath($TestPath)
  return $NormalizedPath
}


# VALIDATES STRING MIGHT BE A PATH AND RETURNS ABSOLUTE PATH #
function ResolvePath($PathName, $TestPath) {
  ValidatePath "$PathName" "$TestPath"
  $ResolvedPath = NormalizePath $PathName $TestPath
  return $ResolvedPath
}

# VALIDATES STRING RESOLVES TO A PATH AND RETURNS ABSOLUTE PATH #
function RequirePath($PathName, $TestPath, $PathType) {
  ValidatePath $PathName $TestPath
  If(!(Test-Path $TestPath -PathType $PathType)) {
    Print -x -f "$PathName ($TestPath) does not exist as a $PathType"
  }
  $ResolvedPath = Resolve-Path $TestPath
  return $ResolvedPath
}

# Like mkdir -p -> creates a directory recursively if it doesn't exist #
function MakeDirP {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$TRUE,Position=0,HelpMessage="Path create.")]
    [string]$Path
  )
  PROCESS {
    New-Item -path $Path -itemtype Directory -force | Out-Null
  }
}

# GETS ALL FILES IN A PATH RECURSIVELY #
function GetFiles {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$TRUE,Position=0,HelpMessage="Path to get files for.")]
    [string]$Path
  )
  PROCESS {
    ls $Path -r | where { !$_.PSIsContainer }
  }
}

# GETS ALL FILES WITH CALCULATED HASH PROPERTY RELATIVE TO A ROOT DIRECTORY RECURSIVELY #
# RETURNS LIST OF @{RelativePath, Hash, FullName}
function GetFilesWithHash {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$TRUE,Position=0,HelpMessage="Path to get directories for.")]
    [string]$Path,

    [parameter(HelpMessage="The hash algorithm to use.")]
    [string]$Algorithm="SHA256"
  )
  PROCESS {
    $OriginalPath = $PWD
    SetWorkDir path/to/diff $Path
    GetFiles $Path | select @{N="RelativePath";E={$_.FullName | Resolve-Path -Relative}},
                            @{N="Hash";E={(Get-FileHash $_.FullName -Algorithm $Algorithm | select Hash).Hash}},
                            FullName
    SetWorkDir path/to/original $OriginalPath
  }
}

function Format-Color([hashtable] $Colors = @{}, [switch] $SimpleMatch) {
  $lines = ($input | Out-String) -replace "`r", "" -split "`n"
  $addedCount = 0
  $deletedCount = 0
	foreach($line in $lines) {
		$color = ''
		foreach($pattern in $Colors.Keys){
			if(!$SimpleMatch -and $line -match $pattern) { $color = $Colors[$pattern] }
			elseif ($SimpleMatch -and $line -like $pattern) { $color = $Colors[$pattern] }
		}
		if($color) {
            if($color -eq 'Red'){
                $deletedCount++
			    Write-Host "`e[31;1m$line`e[0m"
            }
            if($color -eq 'Green'){
                $addedCount++
			    Write-Host "`e[32;1m$line`e[0m"
            }
		} else {
			Write-Host $line
		}
	}
  $annotation=$null
  if($addedCount -gt 0 -or $deletedCount -gt 0){
      $msg=''
      if($addedCount -gt 0) {$msg+="$addedCount new file(s) have been added"}
      if($addedCount -gt 0 -and $deletedCount -gt 0){$msg+=' and '}
      if($deletedCount -gt 0) {$msg+="$deletedCount file(s) have been deleted!"}

      $msg="Files Added/Deleted::$msg"
      $annotation=":warning: **[$DiffBuildSource]** - $msg"
      Write-Host "::warning title=($DiffBuildSource) $msg"
  } else{
      Write-Host "::notice title=($DiffBuildSource) No New Files Added/Deleted::The job has detected that no new files were added or deleted."
  }
  $global:storeResult=$annotation
}

filter UpdatedOnlyFilter{
param(
        [Parameter(Position=0, Mandatory=$true,ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        [PSCustomObject]
        $obj
    )

    $obj|?{$_.Status -ne '--'}
}

# COMPARE TWO DIRECTORIES RECURSIVELY #
# RETURNS LIST OF @{RelativePath, Hash, FullName}
function DiffDirectories {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$TRUE,Position=0,HelpMessage="Directory to compare left.")]
    [alias("l")]
    [string]$LeftPath,

    [parameter(Mandatory=$TRUE,Position=1,HelpMessage="Directory to compare right.")]
    [alias("r")]
    [string]$RightPath
  )
  PROCESS {
    $LeftHash = GetFilesWithHash $LeftPath
    $RightHash = GetFilesWithHash $RightPath
    diff -ReferenceObject $LeftHash -DifferenceObject $RightHash -Property RelativePath,Hash | 
    Select-Object RelativePath, @{
    Name = 'Status'
    Expression = {
        if ($_.SideIndicator -eq "=>") {
          if (-not (Test-Path (Join-Path -Path $LeftPath -ChildPath $_.RelativePath))) {
                'Added'
          }
          else {
                'Modified'
          }
        }
        if ($_.SideIndicator -eq "<=") {
          if (-not (Test-Path (Join-Path -Path $RightPath -ChildPath $_.RelativePath))) {
              'Deleted'
          }
          else {
                '--'
          }
        }
      }
    } | UpdatedOnlyFilter | Sort-Object Status | Format-Color @{'Added' = 'Green'; 'Deleted' = 'Red'}
  }
}


### END FUNCTION DEFINITIONS ###

### PROGRAM LOGIC ###

if($Compare.length -ne 2) {
  Print -x "Compare requires passing exactly 2 path parameters separated by comma, you passed $($Compare.length)." -f
}
Print "Comparing $($Compare[0]) to $($Compare[1])..." -a 1
$LeftPath   = RequirePath path/to/left $Compare[0] container
$RightPath  = RequirePath path/to/right $Compare[1] container
$Diff       = DiffDirectories $LeftPath $RightPath
$LeftDiff   = $Diff | where {$_.SideIndicator -eq "<="} | select RelativePath,Hash
$RightDiff   = $Diff | where {$_.SideIndicator -eq "=>"} | select RelativePath,Hash

"$global:storeResult" > result.txt

if($ExportSummary) {
  $ExportSummary = ResolvePath path/to/summary/dir $ExportSummary
  MakeDirP $ExportSummary
  $SummaryPath = Join-Path $ExportSummary summary.txt
  $LeftCsvPath = Join-Path $ExportSummary left.csv
  $RightCsvPath = Join-Path $ExportSummary right.csv

  $LeftMeasure = $LeftDiff | measure
  $RightMeasure = $RightDiff | measure

  "== DIFF SUMMARY ==" > $SummaryPath
  "" >> $SummaryPath
  "-- DIRECTORIES --" >> $SummaryPath
  "`tLEFT -> $LeftPath" >> $SummaryPath
  "`tRIGHT -> $RightPath" >> $SummaryPath
  "" >> $SummaryPath
  "-- DIFF COUNT --" >> $SummaryPath
  "`tLEFT -> $($LeftMeasure.Count)" >> $SummaryPath
  "`tRIGHT -> $($RightMeasure.Count)" >> $SummaryPath
  "" >> $SummaryPath
  $Diff | Format-Table >> $SummaryPath

  $LeftDiff | Export-Csv $LeftCsvPath -f
  $RightDiff | Export-Csv $RightCsvPath -f
}
$Diff
SafeExit