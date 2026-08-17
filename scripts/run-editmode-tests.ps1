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
New-Item -ItemType Directory -Force -Path $LogsPath | Out-Null

if (Test-Path -LiteralPath $TestResultsPath) {
    Remove-Item -LiteralPath $TestResultsPath -Force
}

$UnityProcess = Start-Process -FilePath $UnityPath -ArgumentList @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", "EditMode",
    "-testResults", $TestResultsPath,
    "-logFile", $EditorLogPath
) -Wait -PassThru -WindowStyle Hidden

$UnityExitCode = $UnityProcess.ExitCode

if ($UnityExitCode -ne 0) {
    exit $UnityExitCode
}

if (-not (Test-Path -LiteralPath $TestResultsPath)) {
    throw "Unity did not produce test results at '$TestResultsPath'. Check '$EditorLogPath' for details."
}
