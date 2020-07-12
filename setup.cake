#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&version=1.1.1

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context, 
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            solutionFilePath: "./Gazorator.sln",
                            title: "Gazorator",
                            repositoryOwner: "mholo65",
                            repositoryName: "gazorator",
                            appVeyorAccountName: "mholo65",
                            shouldRunDupFinder: false,
                            shouldRunInspectCode: false,
                            shouldRunDotNetCorePack: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            dupFinderExcludePattern: new string[] { 
                                BuildParameters.RootDirectoryPath + "/src/Gazorator/**/*.AssemblyInfo.cs"});

Build.RunDotNetCore();