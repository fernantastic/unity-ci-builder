using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityCloudBuild.Editor
{
    public class BuildConfigurationWindow : EditorWindow
    {
        private const string ConfigPath = ".github/workflows/build-config.yml";
        
        // Settings
        private string buildBranches = "main";
        private bool buildWindows64 = true;
        private bool buildMac = true;
        private bool buildLinux = false;
        private bool buildAndroid = false;
        private bool buildiOS = false;
        
        private bool itchEnabled = true;
        private string itchUsername = "user";
        private string itchGameName = "game";
        
        private bool steamEnabled = false;
        private string steamAppId = "";
        private string steamDepotId = "";

        private Vector2 scrollPos;

        [MenuItem("Tools/Unity CI Builder/Open Configuration Window", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<BuildConfigurationWindow>("CI/CD Config");
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Unity CI/CD Builder Configuration", EditorStyles.boldLabel);
            
            // Check if config file exists
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), ConfigPath);
            bool configExists = File.Exists(fullPath);

            // 1. Generate Build Files Button
            GUI.backgroundColor = configExists ? new Color(1f, 0.9f, 0.4f) : new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("1. Generate Build Files", GUILayout.Height(40)))
            {
                CloudBuildSetup.InstallConfigFiles();
                LoadConfig(); // Reload in case it was created/overwritten
            }
            GUI.backgroundColor = Color.white;

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
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space();
            GUILayout.Label("CI Settings", EditorStyles.boldLabel);
            buildBranches = EditorGUILayout.TextField("Build Branches", buildBranches);
            EditorGUILayout.HelpBox("Comma-separated list of branches to build (e.g., 'main, develop').", MessageType.None);

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
                EditorGUILayout.HelpBox($"Target: {itchUsername}/{itchGameName}:[platform]", MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Steam Deployment", EditorStyles.boldLabel);
            steamEnabled = EditorGUILayout.Toggle("Enable Steam", steamEnabled);
            if (steamEnabled)
            {
                EditorGUI.indentLevel++;
                steamAppId = EditorGUILayout.TextField("App ID", steamAppId);
                steamDepotId = EditorGUILayout.TextField("Depot ID", steamDepotId);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Save Configuration File", GUILayout.Height(30)))
            {
                SaveConfig();
            }
            EditorGUILayout.HelpBox("This will save to .github/workflows/build-config.yml", MessageType.None);
            
            if (GUILayout.Button("Reload from File"))
            {
                LoadConfig();
            }

            EditorGUILayout.EndScrollView();
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

            buildWindows64 = ParseBool(content, "BUILD_WINDOWS_64");
            buildMac = ParseBool(content, "BUILD_MAC");
            buildLinux = ParseBool(content, "BUILD_LINUX");
            buildAndroid = ParseBool(content, "BUILD_ANDROID");
            buildiOS = ParseBool(content, "BUILD_IOS");
            
            itchEnabled = ParseBool(content, "ITCH_ENABLED");
            itchUsername = ParseString(content, "ITCH_USERNAME");
            itchGameName = ParseString(content, "ITCH_GAME_NAME");
            
            steamEnabled = ParseBool(content, "STEAM_ENABLED");
            steamAppId = ParseString(content, "STEAM_APP_ID");
            steamDepotId = ParseString(content, "STEAM_DEPOT_ID");
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

            string content = $@"# Unity CI/CD Builder Configuration
# Edit these settings to configure your builds and deployments

# CI Settings
BUILD_BRANCHES: ""{buildBranches}""

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

# Steam Deployment Settings
STEAM_ENABLED: ""{steamEnabled.ToString().ToLower()}""
STEAM_APP_ID: ""{steamAppId}""
STEAM_DEPOT_ID: ""{steamDepotId}""
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

