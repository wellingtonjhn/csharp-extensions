var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactsDirectory = MakeAbsolute(Directory("./artifacts"));

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
.Does(() =>
{
    foreach (var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCorePack(
            project.GetDirectory().FullPath,
            new DotNetCorePackSettings()
            {
                Configuration = configuration,
                OutputDirectory = artifactsDirectory
            });
    }
});

Task("Push-Nuget-Package")
.IsDependentOn("Create-Nuget-Package")
.Does(() =>
{
    var apiKey = "b5819ebc-3fd4-4e98-b391-665f36aff91e"; //EnvironmentVariable("apiKey");
    
    foreach (var package in GetFiles($"{artifactsDirectory}/*.nupkg"))
    {
        NuGetPush(package, 
            new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            });
    }
});

Task("Default").IsDependentOn("Push-Nuget-Package");

RunTarget(target);