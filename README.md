# 🚀 Unity CI/CD Builder

Simple, automated headless Unity builds and deployment. Supports Windows, Mac, Linux, iOS, Android.

## Quick Start (Project Setup)

### 1. Install Package
Add this to your `Packages/manifest.json` dependencies:
```json
"com.fernantastic.unity-ci-builder": "git+https://github.com/YourUser/Unity-CI-Builder.git#main"
```

### 2. Setup Config
In Unity, click **Tools > Unity CI Builder > Install Config Files**.
This sets up:
- `.github/workflows/main_build.yml` (GitHub Actions workflow)
- `Assets/Unity-CI-Builder/Editor/CloudBuild.cs` (Build script)
- `Unity-CI-Builder/Scripts/` (Deployment helper scripts in project root)

### 3. Configure
1. **GitHub Secrets**: Add `ITCHIO_API_KEY`, `STEAM_USER`, `STEAM_PASS` if deploying.
2. **Workflow File**: Open `.github/workflows/main_build.yml`:
   - Set `UNITY_VERSION` to match your project.
   - Update `matrix.targetPlatform` with platforms you want (e.g., `[StandaloneWindows64, Android]`).
   - Uncomment Itch/Steam deploy steps if needed.

## Build Machine Setup (Runner)

To use your own computer as the build server (Self-Hosted Runner), follow these steps:

### 1. Requirements
- **OS**: Windows 10/11 (Preferred for Windows/Xbox builds) or macOS (Required for iOS/Mac builds).
- **Software**:
  - [Unity Hub](https://unity.com/download) & Unity Editor (Install the version used in your project).
  - [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows) (Usually pre-installed on Windows).
  - [Git](https://git-scm.com/downloads).
- **Deployment Tools (Optional)**:
  - [Butler](https://itch.io/docs/butler/) (itch.io CLI): Add to system PATH.
  - [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD): Add to system PATH.

### 2. Configure GitHub Runner
1. Go to your GitHub Repository -> **Settings** -> **Actions** -> **Runners**.
2. Click **New self-hosted runner**.
3. Select your OS (Windows/macOS) and follow the commands provided by GitHub to download and configure the runner agent.
4. **Crucial**: When asked for labels, add `self-hosted` and `windows` (or `macos`). These must match the `runs-on` tags in your `.github/workflows/main_build.yml`.
5. Run the runner (e.g., `.\run.cmd`). For production, install it as a service so it starts automatically.

### 3. Verify Environment
- Ensure Unity is installed at the standard path (e.g., `C:\Program Files\Unity\Hub\Editor\...\Unity.exe`). If not, update the path in `main_build.yml`.
- If deploying to Steam, ensure you have a VDF file and update `VDF_PATH` env var in the workflow.

## Defaults
- **Build Output**: `ProjectRoot/Build/Automated Builds/Latest/[Platform]/`
- **Build Method**: `UnityCloudBuild.CloudBuild.BuildAll` (handles all platforms based on env var)
