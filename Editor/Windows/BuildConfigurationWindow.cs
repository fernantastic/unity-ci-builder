using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityCloudBuild.Editor
{
    public class BuildConfigurationWindow : EditorWindow
    {
        private const string ConfigPath = ".github/build-config.yml";
        
        // Settings
        private string buildBranches = "main";
        private string unityVersion = "2022.3.20f1";
        private bool autoDetectUnityVersion; // Loaded from EditorPrefs
        private bool buildWindows64 = true;
        private bool buildMac = true;
        private bool buildLinux = false;
        private bool buildAndroid = false;
        private bool buildiOS = false;
        
        private bool itchEnabled = true;
        private string itchUsername = "user";
        private string itchGameName = "game";
        private string itchTag = "daily";
        
        private bool steamEnabled = false;
        private string steamAppId = "";
        private string steamDepotId = "";
        private string steamUsername = "";
        private bool steamSetLive = false;
        private string steamSetLiveBranch = "";

        private bool showMore = false;

        private Vector2 scrollPos;

        [MenuItem("Tools/Unity CI Builder/Open Configuration Window", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<BuildConfigurationWindow>("CI/CD Config");
        }

        private void OnEnable()
        {
            autoDetectUnityVersion = EditorPrefs.GetBool("UnityCIBuilder_AutoDetectUnityVersion", true);
            LoadConfig();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.Space();
            GUILayout.Label("Unity CI/CD Builder Configuration", EditorStyles.boldLabel);
            
            // Check required files
            DrawFileStatus();

            // Check if config file exists
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), ConfigPath);
            bool configExists = File.Exists(fullPath);

            // 1. Generate Build Files Button
            GUI.backgroundColor = configExists ? new Color(1f, 0.9f, 0.4f) : new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("1. Generate All Build Files", GUILayout.Height(40)))
            {
                CloudBuildSetup.InstallConfigFiles();
                LoadConfig(); // Reload in case it was created/overwritten
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            if (configExists)
            {
                EditorGUILayout.HelpBox("Build files found. You can update them by clicking above.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Build files not found. Click above to generate them.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox($"Editing: {ConfigPath}", MessageType.None);
            
            EditorGUILayout.Space();
            GUILayout.Label("CI Settings", EditorStyles.boldLabel);
            buildBranches = EditorGUILayout.TextField("Build Branches", buildBranches);
            EditorGUILayout.HelpBox("Comma-separated list of branches to build (e.g., 'main, develop').", MessageType.None);
            
            EditorGUILayout.Space();
            bool newAutoDetect = EditorGUILayout.Toggle("Automatically include Unity version", autoDetectUnityVersion);
            if (newAutoDetect != autoDetectUnityVersion)
            {
                autoDetectUnityVersion = newAutoDetect;
                EditorPrefs.SetBool("UnityCIBuilder_AutoDetectUnityVersion", autoDetectUnityVersion);
            }
            
            if (autoDetectUnityVersion)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Unity Version", Application.unityVersion);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox($"Auto-detected Unity version: {Application.unityVersion}", MessageType.None);
            }
            else
            {
                unityVersion = EditorGUILayout.TextField("Unity Version", unityVersion);
                EditorGUILayout.HelpBox("Unity version to use for builds (e.g., '2022.3.20f1').", MessageType.None);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Build Platforms", EditorStyles.boldLabel);
            buildWindows64 = EditorGUILayout.Toggle("Windows 64-bit", buildWindows64);
            buildMac = EditorGUILayout.Toggle("macOS (Universal)", buildMac);
            buildLinux = EditorGUILayout.Toggle("Linux 64-bit", buildLinux);
            buildAndroid = EditorGUILayout.Toggle("Android", buildAndroid);
            buildiOS = EditorGUILayout.Toggle("iOS", buildiOS);

            EditorGUILayout.Space();
            GUILayout.Label("Itch.io Deployment", EditorStyles.boldLabel);
            itchEnabled = EditorGUILayout.Toggle("Enable Itch.io", itchEnabled);
            if (itchEnabled)
            {
                EditorGUI.indentLevel++;
                itchUsername = EditorGUILayout.TextField("Username", itchUsername);
                itchGameName = EditorGUILayout.TextField("Game Name", itchGameName);
                itchTag = EditorGUILayout.TextField("Channel Tag", itchTag);
                
                string exampleChannel = string.IsNullOrEmpty(itchTag) ? "windows" : $"windows-{itchTag}";
                EditorGUILayout.HelpBox($"Target: {itchUsername}/{itchGameName}:{exampleChannel}", MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Steam Deployment", EditorStyles.boldLabel);
            steamEnabled = EditorGUILayout.Toggle("Enable Steam", steamEnabled);
            if (steamEnabled)
            {
                EditorGUI.indentLevel++;
                steamUsername = EditorGUILayout.TextField("Username", steamUsername);
                steamAppId = EditorGUILayout.TextField("App ID", steamAppId);
                steamDepotId = EditorGUILayout.TextField("Depot ID", steamDepotId);
                
                steamSetLive = EditorGUILayout.Toggle("Set Live", steamSetLive);
                if (steamSetLive)
                {
                    EditorGUI.indentLevel++;
                    steamSetLiveBranch = EditorGUILayout.TextField("Branch Name", steamSetLiveBranch);
                    EditorGUILayout.HelpBox("This branch will be set live immediately after upload.", MessageType.Info);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Save Configuration File", GUILayout.Height(30)))
            {
                SaveConfig();
            }
            EditorGUILayout.HelpBox("This will save to .github/build-config.yml", MessageType.None);
            
            if (GUILayout.Button("Reload from File"))
            {
                LoadConfig();
            }

            EditorGUILayout.Space(20);
            showMore = EditorGUILayout.Foldout(showMore, "More", true);
            if (showMore)
            {
                GUILayout.Label("Test Builds (Local)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Triggers the same build script used by CI/CD. Output: Build/Automated Builds/Latest/", MessageType.Info);
                
                if (GUILayout.Button("Build Windows 64-bit")) RunBuild("StandaloneWindows64");
                if (GUILayout.Button("Build macOS")) RunBuild("StandaloneOSX");
                if (GUILayout.Button("Build Linux 64-bit")) RunBuild("StandaloneLinux64");
                if (GUILayout.Button("Build Android")) RunBuild("Android");
                if (GUILayout.Button("Build iOS")) RunBuild("iOS");
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFileStatus()
        {
            EditorGUILayout.LabelField("Required Files Status:", EditorStyles.boldLabel);
            
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            
            DrawSingleFileStatus("Main Workflow (.github/workflows/main_build.yml)", 
                Path.Combine(projectRoot, ".github/workflows/main_build.yml"));
                
            DrawSingleFileStatus("Build Config (.github/build-config.yml)", 
                Path.Combine(projectRoot, ".github/build-config.yml"));
                
            DrawSingleFileStatus("Itch Deploy Script (.github/scripts/deploy_itch.sh)", 
                Path.Combine(projectRoot, ".github/scripts/deploy_itch.sh"));
                
            DrawSingleFileStatus("Steam Deploy Script (.github/scripts/deploy_steam.sh)", 
                Path.Combine(projectRoot, ".github/scripts/deploy_steam.sh"));
                
            DrawSingleFileStatus("CloudBuild Script (Assets/Unity-CI-Builder/Editor/CloudBuild.cs)", 
                Path.Combine(Application.dataPath, "Unity-CI-Builder/Editor/CloudBuild.cs"));
                
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
        }

        private void DrawSingleFileStatus(string label, string path)
        {
            bool exists = File.Exists(path);
            
            EditorGUILayout.BeginHorizontal();
            
            // Icon
            var icon = exists ? EditorGUIUtility.IconContent("TestPassed") : EditorGUIUtility.IconContent("TestFailed");
            GUILayout.Label(icon, GUILayout.Width(20));
            
            // Label
            GUILayout.Label(label, exists ? EditorStyles.label : EditorStyles.boldLabel);
            
            // Create Button if missing
            if (!exists)
            {
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    // Call the specific install/copy method for this file
                    // For now, we reuse the main install method but we could split it up
                    // Or just trigger the full install since dependencies are interlinked
                    if (EditorUtility.DisplayDialog("Create File", 
                        $"To create this file, we recommend running the full 'Generate All Build Files' process. Proceed?", "Yes", "No"))
                    {
                        CloudBuildSetup.InstallConfigFiles();
                        AssetDatabase.Refresh();
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void RunBuild(string target)
        {
            Debug.Log($"Running test build for {target}...");
            Environment.SetEnvironmentVariable("BUILD_TARGET", target);
            
            // Use reflection to find the CloudBuild class and BuildAll method
            var cloudBuildType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == "UnityCloudBuild.CloudBuild");

            if (cloudBuildType == null)
            {
                EditorUtility.DisplayDialog("Error", "CloudBuild script not found. Please click '1. Generate Build Files' first to install the build scripts.", "OK");
                return;
            }

            var buildMethod = cloudBuildType.GetMethod("BuildAll", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (buildMethod == null)
            {
                Debug.LogError("CloudBuild.BuildAll method not found.");
                return;
            }

            buildMethod.Invoke(null, null);
        }

        private void LoadConfig()
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), ConfigPath);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Config file not found at {fullPath}. Using defaults.");
                return;
            }

            string content = File.ReadAllText(fullPath);
            
            // Simple regex parsing
            buildBranches = ParseString(content, "BUILD_BRANCHES");
            if (string.IsNullOrEmpty(buildBranches)) buildBranches = "main";
            
            string savedUnityVersion = ParseString(content, "UNITY_VERSION");
            if (autoDetectUnityVersion)
            {
                unityVersion = Application.unityVersion;
            }
            else if (!string.IsNullOrEmpty(savedUnityVersion))
            {
                unityVersion = savedUnityVersion;
            }
            else
            {
                unityVersion = Application.unityVersion;
            }

            buildWindows64 = ParseBool(content, "BUILD_WINDOWS_64");
            buildMac = ParseBool(content, "BUILD_MAC");
            buildLinux = ParseBool(content, "BUILD_LINUX");
            buildAndroid = ParseBool(content, "BUILD_ANDROID");
            buildiOS = ParseBool(content, "BUILD_IOS");
            
            itchEnabled = ParseBool(content, "ITCH_ENABLED");
            itchUsername = ParseString(content, "ITCH_USERNAME");
            itchGameName = ParseString(content, "ITCH_GAME_NAME");
            itchTag = ParseString(content, "ITCH_TAG");
            if (string.IsNullOrEmpty(itchTag)) itchTag = "daily";
            
            steamEnabled = ParseBool(content, "STEAM_ENABLED");
            steamUsername = ParseString(content, "STEAM_USERNAME");
            steamAppId = ParseString(content, "STEAM_APP_ID");
            steamDepotId = ParseString(content, "STEAM_DEPOT_ID");
            steamSetLive = ParseBool(content, "STEAM_SET_LIVE");
            steamSetLiveBranch = ParseString(content, "STEAM_SET_LIVE_BRANCH");
        }

        private void SaveConfig()
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), ConfigPath);
            
            if (File.Exists(fullPath))
            {
                if (!EditorUtility.DisplayDialog("Save Configuration", 
                    "Overwrite existing configuration file?", "Yes", "No"))
                {
                    return;
                }
            }
            else
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }

            string versionToSave = autoDetectUnityVersion ? Application.unityVersion : unityVersion;
            
            string content = $@"# Unity CI/CD Builder Configuration
# Edit these settings to configure your builds and deployments

# CI Settings
BUILD_BRANCHES: ""{buildBranches}""

# Unity Version
UNITY_VERSION: ""{versionToSave}""

# Platform Build Settings (use ""true"" or ""false"")
BUILD_WINDOWS_64: ""{buildWindows64.ToString().ToLower()}""
BUILD_MAC: ""{buildMac.ToString().ToLower()}""
BUILD_LINUX: ""{buildLinux.ToString().ToLower()}""
BUILD_ANDROID: ""{buildAndroid.ToString().ToLower()}""
BUILD_IOS: ""{buildiOS.ToString().ToLower()}""

# Itch.io Deployment Settings
ITCH_ENABLED: ""{itchEnabled.ToString().ToLower()}""
ITCH_USERNAME: ""{itchUsername}""
ITCH_GAME_NAME: ""{itchGameName}""
ITCH_TAG: ""{itchTag}""
BUTLER_PATH_WIN: ""%APPDATA%\itch\apps\butler\butler.exe""
BUTLER_PATH_OSX: ""~/.config/itch/apps/butler/butler""

# Steam Deployment Settings
STEAM_ENABLED: ""{steamEnabled.ToString().ToLower()}""
STEAM_USERNAME: ""{steamUsername}""
STEAM_APP_ID: ""{steamAppId}""
STEAM_DEPOT_ID: ""{steamDepotId}""
STEAM_SET_LIVE: ""{steamSetLive.ToString().ToLower()}""
STEAM_SET_LIVE_BRANCH: ""{steamSetLiveBranch}""
";
            File.WriteAllText(fullPath, content);
            Debug.Log($"Configuration saved to: {fullPath}");
            AssetDatabase.Refresh();
        }

        private bool ParseBool(string content, string key)
        {
            var match = Regex.Match(content, $@"{key}:\s*""?(true|false)""?", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return bool.Parse(match.Groups[1].Value);
            }
            return false;
        }

        private string ParseString(string content, string key)
        {
            var match = Regex.Match(content, $@"{key}:\s*""(.*?)""");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            // Fallback for unquoted strings if needed, though we write quoted
            match = Regex.Match(content, $@"{key}:\s*(\S+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "";
        }
    }
}

