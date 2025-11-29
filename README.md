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
1. **GitHub Secrets**: Go to your repository's **Settings > Security > Secrets and variables > Actions** and add:
   - `ITCHIO_API_KEY` (if deploying to Itch)
   - `STEAM_USER`, `STEAM_PASS` (if deploying to Steam)
2. **Configuration Settings**: Edit `.github/workflows/build-config.yml` to configure your builds:
   ```yaml
   BUILD_WINDOWS_64: "true"
   BUILD_MAC: "true"
   BUILD_LINUX: "false"
   BUILD_ANDROID: "false"
   BUILD_IOS: "false"
   
   ITCH_USERNAME: "your-username"
   ITCH_GAME_NAME: "your-game-name"
   ITCH_ENABLED: "true"
   
   STEAM_APP_ID: ""
   STEAM_DEPOT_ID: ""
   STEAM_ENABLED: "false"
   ```
   - Use `"true"` or `"false"` as strings
   - `BUILD_MAC` builds for StandaloneOSX (universal Mac builds - works for both Intel and Apple Silicon)
   - `UNITY_VERSION` is auto-detected. If you upgrade Unity, use **Tools > Unity CI Builder > Update Unity Version in Workflow**.

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
4. **Crucial**: When asked for labels, add `self-hosted`, `windows` (or `macos`), and `x64`. These must match the `runs-on` tags in your `.github/workflows/main_build.yml`.
5. **Run the runner** (choose one option):
   - **Option A - Manual Run** (No admin needed): Simply run `.\run.cmd` in the runner directory. The runner will work but won't start automatically on reboot.
   - **Option B - User Service** (No admin needed): Run `.\svc.cmd install` to install as a user service. It will start automatically when you log in.
   - **Option C - System Service** (Admin required): Right-click PowerShell/Command Prompt, select "Run as Administrator", then run `.\svc.cmd install` and `.\svc.cmd start`. This runs even when you're not logged in.

### 3. Verify Environment
- Ensure Unity is installed at the standard path (e.g., `C:\Program Files\Unity\Hub\Editor\...\Unity.exe`). If not, update the path in `main_build.yml`.

## Deployment Setup

**Prerequisites**: Ensure `butler` (itch.io) and `steamcmd` (Steam) are installed and in your system PATH on the build machine.

### Itch.io Deployment

1. **Get API Key**: Go to [itch.io Settings → API keys](https://itch.io/user/settings/api-keys) and generate a new key.
2. **Add Secret**: Add `ITCHIO_API_KEY` to your repository secrets.
3. **Configure Settings**: In `.github/workflows/main_build.yml`, set:
   - `ITCH_USERNAME`: Your itch.io username
   - `ITCH_GAME_NAME`: Your game name
   - `ITCH_ENABLED: true`

### Steam Deployment

1. **Get Credentials**: Use your Steamworks build account (not personal account).
2. **Add Secrets**: Add `STEAM_USER` and `STEAM_PASS` to your repository secrets.
3. **Configure Settings**: Edit `.github/workflows/build-config.yml`:
   - `STEAM_APP_ID`: Your Steam App ID (as string, e.g., `"123456"`)
   - `STEAM_DEPOT_ID`: Your Steam Depot ID (as string, e.g., `"123457"`)
   - `STEAM_ENABLED: "true"`
   
   The VDF file is automatically generated during the workflow - no manual file editing needed!

## Defaults
- **Build Output**: `ProjectRoot/Build/Automated Builds/Latest/[Platform]/`
- **Build Method**: `UnityCloudBuild.CloudBuild.BuildAll` (handles all platforms based on env var)
