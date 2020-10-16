#addin nuget:?package=Cake.ExtendedNuGet&version=2.1.1
#addin nuget:?package=Cake.FileHelpers&version=3.3.0

#addin nuget:?package=Cake.Issues&version=0.9.1
#addin nuget:?package=Cake.Issues.MsBuild&version=0.9.0
#addin nuget:?package=Cake.Issues.PullRequests&version=0.9.1
#addin nuget:?package=Cake.Issues.PullRequests.GitHubActions&version=0.9.0

using System.Linq;

var target = Argument("target", "Test");
var nugetApiKey = Argument("nugetApiKey", string.Empty);
var configuration = Argument("configuration", "Debug");

var rootDirectory = new DirectoryPath("..");
var artifactsDirectory = rootDirectory.Combine("artifacts");
var library = rootDirectory.CombineWithFilePath("osu.Framework.Live2D/osu.Framework.Live2D.csproj");
var tests = rootDirectory.CombineWithFilePath("osu.Framework.Live2D.Tests/osu.Framework.Live2D.Tests.csproj");
var logs = rootDirectory.CombineWithFilePath("build/logs/msbuild.binlog");

var name = "osu.Framework.Live2D";

var today = DateTime.Today;
var version = "0.0.0";

Task("DetermineVersion")
    .Does(() =>
    {
        var year = today.Year.ToString();
        var monthDay = $"{today.Month.ToString().PadLeft(2, '0')}{today.Day.ToString().PadLeft(2, '0')}";
        if (IsNuGetPublished(name, $"{year}.{monthDay}.0"))
        {
            var latest = NuGetList(name).FirstOrDefault().Version.Split('.');
            if ((year == latest[0]) && (monthDay == latest[1]))
            {
                if (Int32.TryParse(latest[2], out int revision))
                {
                    revision += 1;
                    version = $"{year}.{monthDay}.{revision.ToString()}";
                }
            }
        }
        else
        {
            version = $"{year}.{monthDay}.0";
        }

        Information(version);
        EnsureDirectoryExists(artifactsDirectory);
        FileWriteText(artifactsDirectory.CombineWithFilePath("version"), version);
    });

Task("Clean")
    .WithCriteria(GitHubActions.IsRunningOnGitHubActions)
    .Does(() =>
    {
        EnsureDirectoryExists(artifactsDirectory);
        CleanDirectory(artifactsDirectory);
    });

Task("Compile")
    .Does(() =>
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings
        {
            BinaryLogger = new MSBuildBinaryLoggerSettings
            {
                Enabled = true,
                FileName = logs.FullPath,
            }
        };

        DotNetCoreBuild(tests.FullPath, new DotNetCoreBuildSettings
        {
            Verbosity = DotNetCoreVerbosity.Minimal,
            Configuration = configuration,
            MSBuildSettings = msBuildSettings
        });
    });

Task("CopyNativeLibs")
    .WithCriteria(GitHubActions.IsRunningOnGitHubActions)
    .IsDependentOn("Compile")
    .Does(() =>
    {
        foreach (var directory in  GetDirectories(rootDirectory.Combine("osu.Framework.Live2D.Tests") + $"/bin/{configuration}/netcoreapp3.1"))
            CopyFiles(GetFiles(rootDirectory + "/vignette.NativeLibs/runtimes/**/*"), directory.Combine("runtimes"), true);
    });

// NOTE: Cake has MSBuild Log File Format 8 and the project has 9
Task("CheckIssues")
    .WithCriteria(GitHubActions.IsRunningOnGitHubActions)
    .IsDependentOn("Compile")
    .Does(() =>
    {
        ReportIssuesToPullRequest(MsBuildIssuesFromFilePath(logs.FullPath, MsBuildBinaryLogFileFormat), GitHubActionsBuilds(), rootDirectory);
    });

Task("Test")
    .IsDependentOn("CopyNativeLibs")
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Logger = "trx",
            Settings = new FilePath("vstestconfig.runsettings"),
            ToolTimeout = TimeSpan.FromHours(10),
            Configuration = configuration,
            ResultsDirectory = new DirectoryPath("logs"),
        };

        DotNetCoreTest(tests.FullPath, settings);
    });

Task("Pack")
    .IsDependentOn("DetermineVersion")
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            OutputDirectory = artifactsDirectory,
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Quiet,
            ArgumentCustomization = args =>
            {
                args.Append($"/p:Version={version}");
                args.Append($"/p:GenerateDocumentationFile=true");

                return args;
            }
        };

        DotNetCorePack(library.FullPath, settings);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

RunTarget(target);
