#tool "nuget:?package=GitVersion.CommandLine"
#addin nuget:?package=Newtonsoft.Json

using Newtonsoft.Json;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactsDirectory = MakeAbsolute(Directory("./artifacts"));

Setup(context =>
{
     CleanDirectory(artifactsDirectory);
});

Task("Build")
.Does(() =>
{
    foreach(var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCoreBuild(
            project.GetDirectory().FullPath, 
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration
            });
    }
});

Task("Test")
.IsDependentOn("Build")
.Does(() =>
{
    foreach(var project in GetFiles("./tests/**/*.csproj"))
    {
        DotNetCoreTest(
            project.GetDirectory().FullPath,
            new DotNetCoreTestSettings()
            {
                Configuration = configuration
            });
    }
});

Task("Create-Nuget-Package")
.IsDependentOn("Test")
.WithCriteria(ShouldRunRelease())
.Does(() =>
{
    var version = GetPackageVersion();

    foreach (var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCorePack(
            project.GetDirectory().FullPath,
            new DotNetCorePackSettings()
            {
               
                Configuration = configuration,
                OutputDirectory = artifactsDirectory,
                ArgumentCustomization = args => args.Append($"/p:Version={version}")
            });
    }
});

Task("Push-Nuget-Package")
.IsDependentOn("Create-Nuget-Package")
.WithCriteria(ShouldRunRelease())
.Does(() =>
{
    var apiKey = EnvironmentVariable("NUGET_API_KEY");
    
    foreach (var package in GetFiles($"{artifactsDirectory}/*.nupkg"))
    {
        NuGetPush(package, 
            new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            });
    }
});

Task("Default")
    .IsDependentOn("Push-Nuget-Package");

RunTarget(target);

private bool ShouldRunRelease() => AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Tag.IsTag;

private string GetPackageVersion()
{
    var gitVersion = GitVersion(new GitVersionSettings {
        RepositoryPath = "."
    });

    Information($"Git Semantic Version: {JsonConvert.SerializeObject(gitVersion)}");
    
    return gitVersion.NuGetVersionV2;
}