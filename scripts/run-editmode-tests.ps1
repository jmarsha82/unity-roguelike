param(
    [string]$UnityPath = $env:UNITY_EDITOR_PATH
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
}

if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity editor was not found at '$UnityPath'. Set UNITY_EDITOR_PATH to the Unity executable."
}

$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$LogsPath = Join-Path $ProjectPath "Logs"
$TestResultsPath = Join-Path $LogsPath "editmode-results.xml"
$EditorLogPath = Join-Path $LogsPath "editmode.log"
$CoverageResultsPath = Join-Path $LogsPath "coverage"
New-Item -ItemType Directory -Force -Path $LogsPath | Out-Null

if (Test-Path -LiteralPath $TestResultsPath) {
    Remove-Item -LiteralPath $TestResultsPath -Force
}

if (Test-Path -LiteralPath $CoverageResultsPath) {
    Remove-Item -LiteralPath $CoverageResultsPath -Recurse -Force
}

$UnityProcess = Start-Process -FilePath $UnityPath -ArgumentList @(
    "-batchmode",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", "EditMode",
    "-testResults", $TestResultsPath,
    "-enableCodeCoverage",
    "-coverageResultsPath", $CoverageResultsPath,
    "-coverageOptions", "generateAdditionalMetrics;generateHtmlReport;assemblyFilters:+Assembly-CSharp",
    "-logFile", $EditorLogPath
) -Wait -PassThru -WindowStyle Hidden

$UnityExitCode = $UnityProcess.ExitCode

if ($UnityExitCode -ne 0) {
    exit $UnityExitCode
}

if (-not (Test-Path -LiteralPath $TestResultsPath)) {
    throw "Unity did not produce test results at '$TestResultsPath'. Check '$EditorLogPath' for details."
}

$SummaryPath = Get-ChildItem -LiteralPath $CoverageResultsPath -Filter "Summary.xml" -Recurse -ErrorAction SilentlyContinue |
    Select-Object -First 1 -ExpandProperty FullName

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    throw "Unity did not produce a coverage summary under '$CoverageResultsPath'. Check '$EditorLogPath' for details."
}

[xml]$CoverageSummary = Get-Content -LiteralPath $SummaryPath
$CoverageNode = Select-Xml -Xml $CoverageSummary -XPath "//*[@line-rate or @linecoverage or @sequenceCoverage]" |
    Select-Object -First 1 -ExpandProperty Node

if ($null -ne $CoverageNode) {
    $LineCoverage = if ($CoverageNode.'line-rate') {
        [double]$CoverageNode.'line-rate' * 100
    } elseif ($CoverageNode.linecoverage) {
        [double]$CoverageNode.linecoverage
    } else {
        [double]$CoverageNode.sequenceCoverage
    }
} elseif ($CoverageSummary.CoverageReport.Summary.Linecoverage) {
    $LineCoverage = [double]$CoverageSummary.CoverageReport.Summary.Linecoverage
} else {
    throw "Could not read line coverage from '$SummaryPath'."
}

$FormattedCoverage = $LineCoverage.ToString("0.##")
Write-Host "Line coverage: $FormattedCoverage%"

if ($LineCoverage -lt 90) {
    throw "Line coverage $FormattedCoverage% is below the 90% minimum."
}
