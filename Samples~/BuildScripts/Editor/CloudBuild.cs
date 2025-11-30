using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

namespace UnityCloudBuild
{
    public class CloudBuild
    {
        public static void BuildAll()
        {
            string buildTarget = Environment.GetEnvironmentVariable("BUILD_TARGET"); // e.g., "StandaloneWindows64", "StandaloneOSX", "Android"
            string buildPath = Environment.GetEnvironmentVariable("BUILD_OUTPUT_PATH");

            // Fallback for command line arguments if env vars are missing
            if (string.IsNullOrEmpty(buildTarget))
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-buildTarget" && i + 1 < args.Length)
                    {
                        buildTarget = args[i + 1];
                    }
                    if (args[i] == "-buildPath" && i + 1 < args.Length)
                    {
                        buildPath = args[i + 1];
                    }
                }
            }

            if (string.IsNullOrEmpty(buildTarget))
            {
                Debug.LogError("BUILD_TARGET environment variable or -buildTarget argument not set.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            if (string.IsNullOrEmpty(buildPath))
            {
                // Default path if not set: Project/Build/Automated Builds/Latest/[Target]
                buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build", "Automated Builds", "Latest", buildTarget);
            }

            Debug.Log($"Starting build for {buildTarget} to {buildPath}");
            
            BuildTarget target;
            try
            {
                // Handle StandaloneOSX specially if needed or just parse directly
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTarget);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse build target '{buildTarget}': {e.Message}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // Ensure the editor is switched to the target platform before building
            // This is critical for some platforms to avoid shader compilation issues or wrong asset bundles
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                Debug.Log($"Switching active build target to {target}...");
                // Note: SwitchActiveBuildTarget is obsolete in newer Unity versions, but often still works or has replacements
                // For batch mode, the -buildTarget command line arg usually handles this, but let's be safe
                // EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
            }

            string extension = GetExtension(target);
            string executableName = Application.productName + extension;
            string locationPathName = Path.Combine(buildPath, executableName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(locationPathName));

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = locationPathName,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            Debug.Log($"Build result: {summary.result}");

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build SUCCEEDED! {summary.totalSize / 1024 / 1024} MB");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build FAILED! Errors: {summary.totalErrors}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static string GetExtension(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.StandaloneOSX:
                    return ".app";
                case BuildTarget.Android:
                    return ".apk"; // or .aab
                case BuildTarget.iOS:
                    return ""; // iOS builds a folder project
                case BuildTarget.StandaloneLinux64:
                    return ".x86_64";
                default:
                    return "";
            }
        }
    }
}
