param(
    [Parameter(Mandatory=$true)]
    [string]$SteamAppID,
    
    [Parameter(Mandatory=$true)]
    [string]$BuildDirectory # For logging/validation, not used directly by SteamCMD command
)

# Ensure SteamCMD is accessible (assumes it's in PATH or referenced by full path)
$SteamCmdPath = "steamcmd" 
$VDFPath = $env:VDF_PATH # Must be passed as an environment variable

if (-not (Get-Command $SteamCmdPath -ErrorAction SilentlyContinue)) {
    Write-Error "SteamCMD command not found. Please ensure steamcmd.exe is in the system PATH."
    exit 1
}

if (-not (Test-Path $VDFPath)) {
    Write-Error "Steam build VDF file not found at $VDFPath. Aborting deployment."
    exit 1
}

Write-Host "--- Deploying to Steam ---"
Write-Host "App ID: $SteamAppID"
Write-Host "VDF Config: $VDFPath"

# STEAM_USER and STEAM_PASS must be set as environment variables (secrets)
& $SteamCmdPath `
  +login $env:STEAM_USER $env:STEAM_PASS `
  +run_app_build $VDFPath `
  +quit

$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "SteamCMD deployment failed with exit code $exitCode"
    exit 1
} else {
    Write-Host "Steam deployment successful!"
}

exit 0

