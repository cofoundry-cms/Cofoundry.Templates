#tool "nuget:?package=GitVersion.CommandLine"

using System.Text.RegularExpressions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var pushPackages = Argument("PushPackages", "false") == "true";
bool isPrerelease = false;
GitVersion versionInfo = null;
var artifactDirectory = "./artifacts";
var nuspecFile = "/Cofoundry.Templates.nuspec";

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
    CopyFile("./src" + nuspecFile, artifactDirectory + nuspecFile);
});

Task("Pack")
    .IsDependentOn("Copy")
    .Does(() =>
{
    
    var settings = new NuGetPackSettings
        {
            OutputDirectory = "./artifacts/",
            Version = versionInfo.NuGetVersion
        };
    
    NuGetPack(artifactDirectory + nuspecFile, settings);
});

Task("PushNuGetPackage")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var nugets = GetFiles(artifactDirectory + "/*.nupkg");
    
    if (pushPackages)
    {
        Information("Pushing packages");
        
        if (isPrerelease)
        {
            NuGetPush(nugets, new NuGetPushSettings {
                Source = "https://www.myget.org/F/cofoundry/api/v2/package",
                ApiKey = EnvironmentVariable("MYGET_API_KEY")
            });
        }
        else
        {
            NuGetPush(nugets, new NuGetPushSettings {
                Source = "https://nuget.org/",
                ApiKey = EnvironmentVariable("NUGET_API_KEY")
            });
        }
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("PushNuGetPackage");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
