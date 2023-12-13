#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#module nuget:?package=Cake.BuildSystems.Module&version=##see below for note on versioning##
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
	Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
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



Task("Default")
.IsDependentOn("Clean")
.IsDependentOn("Restore-NuGet-Packages")
.IsDependentOn("Build")
.Does(() => {
	Information("Hello Cake!");
});

RunTarget(target);