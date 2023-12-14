//#addin nuget:?package=NUnit.ConsoleRunner&version=3.16.3
//#tool nuget:?package=NUnit.ConsoleRunner&version=3.16.3
//#tool nuget:?package=NUnit.Extension.TeamCityEventListener
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools&version=2023.3.1"
#module "nuget:?package=Cake.BuildSystems.Module&version=6.1.0"
#addin nuget:?package=Cake.Coverlet&version=3.0.4
#tool dotnet:?package=coverlet.console&version=6.0.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var restoreSettings = new DotNetRestoreSettings
 {
     Sources = new[] {"https://api.nuget.org/v3/index.json"},
     PackagesDirectory = "./packages",
     DisableParallel = true
 };



///////////////////////////////////////////////////////////////////////////////
// PARAMETERS
///////////////////////////////////////////////////////////////////////////////

var artifactsFolder = "./artifacts";
var temporaryFolder = "./temp-build";
var solution = "./CakeToolChainTest.sln";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
	// Executed BEFORE the first task.
	//Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	//Information("Finished running tasks.");
});

TaskSetup(ctx =>
{
	if(TeamCity.IsRunningOnTeamCity){
		TeamCity.WriteStartProgress(ctx.Task.Description ?? ctx.Task.Name);
		}
});
TaskTeardown(ctx =>
{
	if(TeamCity.IsRunningOnTeamCity){
		TeamCity.WriteEndProgress(ctx.Task.Description ?? ctx.Task.Name);
		}
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Description("Cleans any build artifacts and folders.")
	.Does(() => {
		if(!DirectoryExists(artifactsFolder)){
			CreateDirectory(artifactsFolder);
		}
		CleanDirectory(artifactsFolder);
		if(!DirectoryExists(temporaryFolder)){
			CreateDirectory(temporaryFolder);
		}
		CleanDirectory(temporaryFolder);
		CleanDirectory(restoreSettings.PackagesDirectory);
	});


Task("Restore-NuGet-Packages")
	.Description("Restores NuGet packages.")
	.Does(() => {
		DotNetRestore(solution);
	});


Task("Build")
	.Description("Builds the solution.")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() => {
		DotNetBuild(solution, new DotNetBuildSettings {
			Configuration = configuration,
			NoRestore = true,
			NoIncremental = true,
			MSBuildSettings = new DotNetMSBuildSettings {
				MaxCpuCount = 1,
				NodeReuse = false,
				Verbosity = DotNetVerbosity.Minimal
			}
		});
	});

Task("TestCoverlet")
.Description("Runs tests.")
.IsDependentOn("Build")
.Does(() => {
		var coverletSettings = new CoverletSettings {
	        CollectCoverage = true,
		    CoverletOutputFormat = CoverletOutputFormat.teamcity,
			CoverletOutputDirectory = Directory(artifactsFolder),
			CoverletOutputName = $"results-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss-FFF}"
		};
		var testSettings = new DotNetTestSettings {};
		DotNetTest("./SimpleDLL.Nunit.Tests/bin/debug/net8.0/SimpleDLL.Nunit.Tests.dll", testSettings,coverletSettings);
		//Coverlet("./SimpleDLL.Nunit.Tests/bin/debug/net8.0/SimpleDLL.Nunit.Tests.dll","./SimpleDLL.Nunit.Tests/SimpleDLL.Nunit.Tests.csproj",coverletSettings);
	
});
Task("TestDotCover")
.Description("Runs tests.")
.IsDependentOn("Build")
.Does(() => {
		var dotCoverCoverSettings = new DotCoverCoverSettings();
			dotCoverCoverSettings.WithAttributeFilter( "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute" );

		var testSettings = new DotNetTestSettings {};
		
		DotCoverCover( (tool) =>{
			tool.DotNetTest("./SimpleDLL.Nunit.Tests/bin/debug/net8.0/SimpleDLL.Nunit.Tests.dll", testSettings);
			},"./artifacts/results.dcvr",dotCoverCoverSettings);
	
});

Task("Default")
.IsDependentOn("Clean")
.IsDependentOn("Restore-NuGet-Packages")
.IsDependentOn("Build")
.IsDependentOn("TestDotCover")
.Does(() => {
	
});

RunTarget(target);