#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&version=0.3.0-unstable0372

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context, 
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            solutionFilePath: "./Gazorator.sln",
                            title: "Gazorator",
                            repositoryOwner: "mholo65",
                            repositoryName: "gazorator",
                            appVeyorAccountName: "mholo65",
                            shouldRunDotNetCorePack: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            dupFinderExcludePattern: new string[] { 
                                BuildParameters.RootDirectoryPath + "/src/Gazorator/**/*.AssemblyInfo.cs"});

Build.RunDotNetCore();