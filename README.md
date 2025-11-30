# 🚀 Unity CI/CD Builder

Simple, automated headless Unity builds and deployment using **GitHub Actions** and **Self-Hosted Runners**.

This tool is added to your Unity project and handles all the necessary scripts and workflows to:
- Trigger builds on your own build machine whenever you push code.
- Support multi-platform builds: Windows, Mac, Linux, iOS, Android.
- Automatically upload builds to **Itch.io** and **Steam**.
- Configure everything directly inside Unity via a **Configuration Window** (or by editing simple config files).

## Quick Start (Project Setup)

### 1. Install Package
Add this package via the Package Manager using "Add by Git URL" and enter: 
```json
https://github.com/fernantastic/unity-ci-builder.git
```

### 2. Setup Config
1. In Unity, go to **Tools > Unity CI Builder > Open Configuration Window**.
2. Click the big button **"1. Generate Build Files"**.
   - This installs the necessary files (workflow, scripts, config) into your project.
3. Configure your build settings in the window:
   - **Build Platforms**: Toggle the platforms you want to build.
   - **Itch.io Deployment**: Enable and enter your itch.io username and game name.
   - **Steam Deployment**: Enable and enter your App ID and Depot ID.
4. Click **"Save Configuration File"**.
   - This saves your settings to `.github/workflows/build-config.yml`.

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
3. **Configure Settings**: Open **Tools > Unity CI Builder > Open Configuration Window**:
   - Enable Itch.io.
   - Enter your **Username** and **Game Name**.
   - Click **Save Configuration File**.

### Steam Deployment

1. **Get Credentials**: Use your Steamworks build account (not personal account).
2. **Add Secrets**: Add `STEAM_USER` and `STEAM_PASS` to your repository secrets.
3. **Configure Settings**: Open **Tools > Unity CI Builder > Open Configuration Window**:
   - Enable Steam.
   - Enter your **App ID** and **Depot ID**.
   - Click **Save Configuration File**.
   
   The VDF file is automatically generated during the workflow using these ID's.

   **Optional: Custom VDF File**
   If you need a complex Steam setup (multiple depots, custom install scripts), you can create a custom VDF file:
   1. Go to **Tools > Unity CI Builder > Scripts > Create Steam VDF Template**.
   2. This creates `Unity-CI-Builder/steam_app_build.vdf`.
   3. Edit this file as needed.
   4. The workflow will detect this file exists and use it *instead* of auto-generating one.

## Managing your runner

Once installed, manage your workflow, via the url `https://github.com/yourusername/yourproject/actions`

## Defaults
- **Build Output**: `ProjectRoot/Build/Automated Builds/Latest/[Platform]/`
- **Build Method**: `UnityCloudBuild.CloudBuild.BuildAll` (handles all platforms based on env var)
