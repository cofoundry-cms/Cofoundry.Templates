#tool "nuget:?package=GitVersion.CommandLine"

using System.Text.RegularExpressions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var pushPackages = Argument("PushPackages", "false") == "true";
bool isPrerelease = false;
GitVersion versionInfo = null;
string artifactDirectory = "./artifacts";

var projectsToBuild = new string[]{
    "Cofoundry.Templates.Web",
};


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

//GitVersion versionInfo = null;
var nugetPackageDir = Directory("./artifacts");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactDirectory);
    CleanDirectories("./src/**/bin/");
    CleanDirectories("./src/**/obj/");
});


Task("Get-Version")
    .IsDependentOn("Clean")
    .Does(() =>
{
    versionInfo = GitVersion(new GitVersionSettings{
        UpdateAssemblyInfo = false
    });

    Information("Building version {0} of Cofoundry Templates.", versionInfo.InformationalVersion);

    isPrerelease = versionInfo.PreReleaseNumber.HasValue;
});

Task("Copy")
    .IsDependentOn("Get-Version")
    .Does(() =>
{
    foreach (var projectToBuild in projectsToBuild)
    {
        var sourceDirectory = "./src/" + projectToBuild + "/";
        var packageDirectory = artifactDirectory + "/" + projectToBuild;
        Information("Copying {0} to {1}", sourceDirectory, packageDirectory);
        CopyDirectory(sourceDirectory, packageDirectory);
    }
});

// Task("Build")
//     .IsDependentOn("Restore-NuGet-Packages")
//     .Does(() =>
// {
    
//     var settings = new DotNetCoreBuildSettings
//         {
//             Configuration = configuration,
//             ArgumentCustomization = args => args
//                 .Append("/p:NuGetVersion=" + versionInfo.NuGetVersion)
//                 .Append("/p:AssemblyVersion=" + versionInfo.AssemblySemVer)
//                 .Append("/p:FileVersion=" + versionInfo.MajorMinorPatch + ".0")
//                 .Append("/p:InformationalVersion=" + versionInfo.InformationalVersion)
//                 .Append("/p:Copyright=" + "\"Copyright © Cofoundry.org " + DateTime.Now.Year + "\"")
//         };
    
//     foreach (var projectToBuild in projectsToBuild)
//     {
//         DotNetCoreBuild(projectToBuild, settings);
//     }
// });

// Task("Pack")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     var settings = new DotNetCorePackSettings
//         {
//             Configuration = configuration,
//             OutputDirectory = "./artifacts/",
//             NoBuild = true
//         };
    
//     foreach (var projectToBuild in projectsToBuild)
//     {
//         DotNetCorePack(projectToBuild, settings);
//     }
// });

// Task("PushNuGetPackage")
//     .IsDependentOn("Pack")
//     .Does(() =>
// {
//     var nugets = GetFiles("./artifacts/*.nupkg");
    
//     if (pushPackages)
//     {
//         Information("Pushing packages");
        
//         if (isPrerelease)
//         {
//             NuGetPush(nugets, new NuGetPushSettings {
//                 Source = "https://www.myget.org/F/cofoundry/api/v2/package",
//                 ApiKey = EnvironmentVariable("MYGET_API_KEY")
//             });
//         }
//         else
//         {
//             NuGetPush(nugets, new NuGetPushSettings {
//                 Source = "https://nuget.org/",
//                 ApiKey = EnvironmentVariable("NUGET_API_KEY")
//             });
//         }
//     }
// });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("Copy");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
