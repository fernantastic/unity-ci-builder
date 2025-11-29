using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityCloudBuild.Editor
{
    public class CloudBuildSetup
    {
        private const string PackageName = "com.fernantastic.unity-ci-builder";

        [MenuItem("Tools/Unity CI Builder/Install Config Files")]
        public static void InstallConfigFiles()
        {
            if (!EditorUtility.DisplayDialog("Install CI/CD Config", 
                "This will copy build scripts and workflow templates to your project. Existing files may be overwritten. Continue?", 
                "Yes", "No"))
            {
                return;
            }

            string packageRoot = GetPackageRootPath();
            if (string.IsNullOrEmpty(packageRoot))
            {
                EditorUtility.DisplayDialog("Error", $"Could not find package '{PackageName}'.", "OK");
                return;
            }

            // 1. Copy PowerShell Scripts to Unity-CI-Builder/Scripts
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string builderRoot = Path.Combine(projectRoot, "Unity-CI-Builder");
            string scriptsDestDir = Path.Combine(builderRoot, "Scripts");
            
            CopyDirectory(
                Path.Combine(packageRoot, "Scripts"), 
                scriptsDestDir
            );

            // 2. Copy Workflow Template
            string workflowSrc = Path.Combine(packageRoot, ".github/workflows/main_build.yml.template");
            string workflowDestDir = Path.Combine(projectRoot, ".github/workflows");
            string workflowDest = Path.Combine(workflowDestDir, "main_build.yml");

            if (File.Exists(workflowSrc))
            {
                Directory.CreateDirectory(workflowDestDir);
                if (!File.Exists(workflowDest) || EditorUtility.DisplayDialog("Overwrite Workflow?", "main_build.yml already exists. Overwrite?", "Yes", "No"))
                {
                    string content = File.ReadAllText(workflowSrc);
                    content = content.Replace(@".\Scripts\", @".\Unity-CI-Builder\Scripts\");
                    
                    // Auto-detect and set Unity version
                    string currentUnityVersion = Application.unityVersion;
                    // Regex to find "UNITY_VERSION: <something>" (simpler regex)
                    content = Regex.Replace(content, @"UNITY_VERSION:.*", $"UNITY_VERSION: {currentUnityVersion}");
                    
                    File.WriteAllText(workflowDest, content);
                    Debug.Log($"Installed workflow to: {workflowDest} with Unity version {currentUnityVersion}");
                }
            }
            else
            {
                Debug.LogError($"Workflow template not found at {workflowSrc}");
            }

            // 2b. Copy Config File
            string configSrc = Path.Combine(packageRoot, ".github/workflows/build-config.yml");
            string configDest = Path.Combine(workflowDestDir, "build-config.yml");

            if (File.Exists(configSrc))
            {
                if (!File.Exists(configDest))
                {
                    File.Copy(configSrc, configDest);
                    Debug.Log($"Installed config file to: {configDest}");
                }
                else
                {
                    Debug.LogWarning($"Config file already exists at {configDest}. Skipping to preserve your settings.");
                }
            }

            // 3. Copy Sample Build Script to Assets/Unity-CI-Builder/Editor/CloudBuild.cs
            string scriptSrc = Path.Combine(packageRoot, "Samples~/BuildScripts/Editor/CloudBuild.cs");
            string editorDestDir = Path.Combine(Application.dataPath, "Unity-CI-Builder/Editor");
            string scriptDest = Path.Combine(editorDestDir, "CloudBuild.cs");

            if (File.Exists(scriptSrc))
            {
                Directory.CreateDirectory(editorDestDir);
                if (!File.Exists(scriptDest))
                {
                    File.Copy(scriptSrc, scriptDest);
                    Debug.Log($"Installed build script to: {scriptDest}");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning($"Build script already exists at {scriptDest}. Skipping copy to avoid overwriting changes.");
                }
            }

            // 4. Generate Steam VDF template (optional - workflow auto-generates it, but this serves as a reference)
            string vdfPath = Path.Combine(builderRoot, "steam_app_build.vdf");
            if (!File.Exists(vdfPath))
            {
                GenerateSteamVDFTemplate(vdfPath, projectRoot);
                Debug.Log($"Generated Steam VDF template at: {vdfPath} (workflow will auto-generate during builds)");
            }

            Debug.Log("Unity CI/CD Builder setup complete.");
        }

        [MenuItem("Tools/Unity CI Builder/Update Unity Version in Workflow")]
        public static void UpdateUnityVersion()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string workflowPath = Path.Combine(projectRoot, ".github/workflows/main_build.yml");

            if (!File.Exists(workflowPath))
            {
                EditorUtility.DisplayDialog("Error", "Workflow file not found. Please install config files first.", "OK");
                return;
            }

            string content = File.ReadAllText(workflowPath);
            string currentUnityVersion = Application.unityVersion;
            
            // Regex to find "UNITY_VERSION: <something>"
            string pattern = @"(UNITY_VERSION:\s*)(.*)";
            string newContent = Regex.Replace(content, pattern, $"$1{currentUnityVersion}");

            if (content != newContent)
            {
                File.WriteAllText(workflowPath, newContent);
                Debug.Log($"Updated Unity version in workflow to {currentUnityVersion}");
                EditorUtility.DisplayDialog("Success", $"Updated Unity version to {currentUnityVersion}", "OK");
            }
            else
            {
                Debug.Log("Unity version in workflow is already up to date.");
                EditorUtility.DisplayDialog("Info", "Unity version is already up to date.", "OK");
            }
        }

        private static string GetPackageRootPath()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(CloudBuildSetup).Assembly);
            if (packageInfo != null) return packageInfo.resolvedPath;
            if (Directory.Exists("Packages/" + PackageName)) return Path.GetFullPath("Packages/" + PackageName);
            return Path.GetFullPath("."); 
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source directory not found: {sourceDir}");
                return;
            }

            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                Debug.Log($"Installed: {destFile}");
            }
        }

        private static void GenerateSteamVDFTemplate(string vdfPath, string projectRoot)
        {
            string buildOutputPath = Path.Combine(projectRoot, "Build", "Automated Builds", "Latest", "StandaloneWindows64");
            string steamOutputPath = Path.Combine(projectRoot, "Build", "SteamOutput");
            
            string vdfContent = @"// Steam App Build Configuration
// Edit this file with your Steam App ID and depot settings
// See: https://partner.steamgames.com/doc/sdk/uploading

""AppBuild""
{
	""AppID"" ""1234560""  // Replace with your Steam App ID
	
	""Desc"" ""Build Description""
	
	""ContentRoot"" """ + buildOutputPath.Replace("\\", "\\\\") + @"""  // Build output directory
	
	""BuildOutput"" """ + steamOutputPath.Replace("\\", "\\\\") + @"""  // Steam upload output
	
	""Depots""
	{
		""1234561""  // Replace with your Depot ID
		{
			""LocalPath"" "".""
			""DepotBuildConfig"" ""depot_build_1234561.vdf""
		}
	}
}
";
            Directory.CreateDirectory(Path.GetDirectoryName(vdfPath));
            File.WriteAllText(vdfPath, vdfContent);
        }
    }
}
