param(
    [Parameter(Mandatory=$true)]
    [string]$BuildDirectory,
    
    [Parameter(Mandatory=$true)]
    [string]$Target,
    
    [Parameter(Mandatory=$true)]
    [string]$Version
)

# Ensure Butler is accessible (assumes it's in PATH or referenced by full path)
$ButlerPath = "butler" 

if (-not (Get-Command $ButlerPath -ErrorAction SilentlyContinue)) {
    Write-Error "Butler command not found. Please ensure butler.exe is in the system PATH."
    exit 1
}

Write-Host "--- Deploying to itch.io ---"
Write-Host "Directory: $BuildDirectory"
Write-Host "Target: $Target"
Write-Host "Version: $Version"

# BUTLER_API_KEY must be set as an environment variable in the GitHub workflow
& $ButlerPath push $BuildDirectory $Target --userversion $Version
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "Butler deployment failed with exit code $exitCode"
    exit 1
} else {
    Write-Host "itch.io deployment successful!"
}

exit 0

