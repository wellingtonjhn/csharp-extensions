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
    var version = GetVersion();

    foreach(var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCoreBuild(
            project.GetDirectory().FullPath, 
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                ArgumentCustomization = args => args.Append($"/p:SemVer={version}")
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

Task("Move-Nuget-Package")
.IsDependentOn("Build")
.WithCriteria(ShouldRunRelease())
.Does(() =>
{
    var files = GetFiles("./src/**/*.nupkg");
    CopyFiles(files, artifactsDirectory);
});

Task("Push-Nuget-Package")
.IsDependentOn("Move-Nuget-Package")
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
    .IsDependentOn("Test")
    .IsDependentOn("Push-Nuget-Package");

RunTarget(target);

private bool ShouldRunRelease() => AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Tag.IsTag;

private string GetVersion()
{
    var gitVersion = GitVersion(new GitVersionSettings {
        RepositoryPath = "."
    });

    Information($"Git Semantic Version: {JsonConvert.SerializeObject(gitVersion)}");
    
    var build = AppVeyor.IsRunningOnAppVeyor ? $"+build.{AppVeyor.Environment.Build.Number.ToString()}" : string.Empty;

    var version = gitVersion.SemVer + build;

    Information($"Version: {version}");

    return version;
}