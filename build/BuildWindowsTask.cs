using Cake.Common.Tools.MSBuild;

namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        //  Disable openmp so there is no dependency on VCOMP140.dll
        context.ReplaceTextInFiles("freeimage/**/*LibRawLite.2017.vcxproj", "<OpenMPSupport>true</OpenMPSupport>", "<OpenMPSupport>false</OpenMPSupport>");

        // Replace std::binary_function with our own as its been deprecated
        context.ReplaceTextInFiles("freeimage/Source/OpenEXR/IlmImf/ImfAttribute.cpp", "std::binary_function", "binary_function");
        context.ReplaceRegexInFiles("freeimage/Source/OpenEXR/IlmImf/ImfAttribute.cpp", "namespace {.*", "namespace { template<class Arg1, class Arg2, class Result> struct binary_function { using first_argument_type = Arg1; using second_argument_type = Arg2; using result_type = Result; };");

        MSBuildSettings buildSettingsx64 = new()
        {
            Verbosity = Verbosity.Normal,
            Configuration = "Release",
            PlatformTarget = PlatformTarget.x64
        };
        buildSettingsx64.WithProperty("WindowsTargetPlatformVersion", "10.0.17763.0");
        buildSettingsx64.WithProperty("PlatformToolset", "v143");
        context.MSBuild("freeimage/FreeImage.2017.sln", buildSettingsx64);
        context.CreateDirectory($"{context.ArtifactsDir}/x64");
        context.CopyFile("freeimage/Dist/x64/Freeimage.dll", $"{context.ArtifactsDir}/x64/FreeImage.dll");

        MSBuildSettings buildSettingsarm64 = new()
        {
            Verbosity = Verbosity.Normal,
            Configuration = "Release",
            PlatformTarget = PlatformTarget.ARM64
        };
        buildSettingsarm64.WithProperty("WindowsTargetPlatformVersion", "10.0.17763.0");
        buildSettingsarm64.WithProperty("PlatformToolset", "v143");
        context.MSBuild("freeimage/FreeImage.2017.sln", buildSettingsarm64);
        context.CreateDirectory($"{context.ArtifactsDir}/arm64");
        context.CopyFile("freeimage/Dist/arm64/Freeimage.dll", $"{context.ArtifactsDir}/arm64/FreeImage.dll");
    }
}
