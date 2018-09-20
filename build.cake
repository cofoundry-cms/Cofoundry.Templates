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

var projectsToBuild = new string[] {
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
        var packageContentDirectory = packageDirectory + "/content";
        var directorySettings = new DeleteDirectorySettings() {
            Recursive = true,
            Force = true
        };
        CopyDirectory(sourceDirectory, packageContentDirectory);

        DeleteDirectory(packageContentDirectory + "/bin", directorySettings);
        DeleteDirectory(packageContentDirectory + "/obj", directorySettings);
        DeleteDirectory(packageContentDirectory + "/App_Data", directorySettings);

        var nuspecFile = "/" + projectToBuild + ".nuspec";
        MoveFile(packageContentDirectory + nuspecFile, packageDirectory + nuspecFile);
    }
});

Task("Pack")
    .IsDependentOn("Copy")
    .Does(() =>
{
    
    var settings = new NuGetPackSettings
        {
            OutputDirectory = "./artifacts/",
            Version = versionInfo.NuGetVersion
            // ArgumentCustomization = args => args
            //     .Append("/p:NuGetVersion=" + versionInfo.NuGetVersion)
            //     .Append("/p:AssemblyVersion=" + versionInfo.AssemblySemVer)
            //     .Append("/p:FileVersion=" + versionInfo.MajorMinorPatch + ".0")
            //     .Append("/p:InformationalVersion=" + versionInfo.InformationalVersion)
            //     .Append("/p:Copyright=" + "\"Copyright © Cofoundry.org " + DateTime.Now.Year + "\"")
        };
    
    foreach (var projectToBuild in projectsToBuild)
    {
        var packageDirectory = artifactDirectory + "/" + projectToBuild;
        var nuspecFile = packageDirectory + "/" + projectToBuild + ".nuspec";
        NuGetPack(nuspecFile, settings);
    }
});

// Task("Pack")
//     .IsDependentOn("Copy")
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

Task("Default").IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
