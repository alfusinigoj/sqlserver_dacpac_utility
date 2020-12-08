$script:project_config = "Release"

properties {

  $solution_name = "Dacpac.Util"
  $domain = ""
  $environment = "Development"
  $app_name = "Dacpac.Util"

  $base_dir = resolve-path .
  $project_dir = "$base_dir\src\$project_name"
  $project_file = "$project_dir\$project_name.csproj"
  $solution_file = "$base_dir\$solution_name.sln"
  $publish_dir = "$base_dir\_publish"
  $release_id = "win-x64"

  $version = get_version
  $date = Get-Date
  $dotnet_exe = get-dotnet
  $ReleaseNumber = $version
  $buildNumber=0
  
  Write-Host "**********************************************************************"
  Write-Host "Release Number: $ReleaseNumber"
  Write-Host "**********************************************************************"
}
   
#These are aliases for other build tasks. They typically are named after the camelcase letters (rd = Rebuild Databases)
task default -depends DeveloperBuild
task ci -depends IntegrationBuild
task ? -depends help
task p -depends PublishLocally

task help {
   Write-Help-Header
   Write-Help-Section-Header "Comprehensive Building"
   Write-Help-For-Alias "(default)" "Intended for first build or when you want a fresh, clean local copy"
   Write-Help-For-Alias "ci" "Continuous Integration build (long and thorough) with packaging"
   Write-Help-For-Alias "test" "Run local tests"
   Write-Help-For-Alias "pr" "Intended for pub;ishing and running the app"
   Write-Help-Footer
   exit 0
}

#These are the actual build tasks. They should be Pascal case by convention
task DeveloperBuild -depends SetDebugBuild, PackageRestore, Clean, Compile, RunTests
#task IntegrationBuild -depends SetReleaseBuild, SetCiProperties, PackageRestore, Clean, Compile, RunTests, Publish
task IntegrationBuild -depends SetReleaseBuild, PackageRestore, Clean, Compile, RunTests, Publish
task PublishLocally -depends SetDebugBuild, PackageRestore, Clean, Compile, RunTests, Publish

task SetDebugBuild {
    $script:project_config = "Debug"
}

task SetReleaseBuild {
    $script:project_config = "Release"
	Push-Location Env:
	[System.Environment]::SetEnvironmentVariable('MY_VERSION', $version, [System.EnvironmentVariableTarget]::User)
	#$env:MY_VERSION = $version
	Write-Host $env:MY_VERSION
	Pop-Location
}

task SetCiProperties {
	Write-Host "******************* Now Setting Ci properties *********************"
    $buildNumber = $env:bamboo_buildNumber
	Write-Host "Build Number: $buildNumber"
}

task Clean {
	if (Test-Path $publish_dir) {
		delete_directory $publish_dir
	}

	Write-Host "******************* Now Cleaning the Solution *********************"
    exec { & $dotnet_exe clean -c $project_config $solution_file }
}

task PackageRestore {
	Write-Host "******************* Now restoring the Solution packages *********************"
	exec { & $dotnet_exe restore $solution_file }
}

task Compile {
	Write-Host "******************* Now Compiling the solution $solution_file *********************"
	exec { & $dotnet_exe build -c $project_config $solution_file}
}

task RunTests {
   Write-Host "******************* Now running Tests *********************"
   Write-Host $dotnet_exe test
   Write-Host $project_config 
   Write-Host "$project_dir.Tests"
   exec { & $dotnet_exe test -c $project_config "$project_dir.Tests" -- xunit.parallelizeTestCollections=true }
}

task Publish {
	Write-Host "******************* Publishing to $publish_dir *********************"
	if (!(Test-Path $publish_dir)) {
		New-Item -ItemType Directory -Force -Path $publish_dir
	}
	$assemblyVersion="$version"+"."+"$buildNumber"
	exec { & $dotnet_exe publish -c $project_config $project_file -o $publish_dir -r $release_id -p:AssemblyVersion=$assemblyVersion -p:Version=$version -p:FileVersion=$assemblyVersion}
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions for Help Section
# --------------------------------------------------------------------------------------------------------------

function Write-Help-Header($description) {
   Write-Host ""
   Write-Host "********************************" -foregroundcolor DarkGreen -nonewline;
   Write-Host " HELP " -foregroundcolor Green  -nonewline; 
   Write-Host "********************************"  -foregroundcolor DarkGreen
   Write-Host ""
   Write-Host "This build script has the following common build " -nonewline;
   Write-Host "task " -foregroundcolor Green -nonewline;
   Write-Host "aliases set up:"
}

function Write-Help-Footer($description) {
   Write-Host ""
   Write-Host " For a complete list of build tasks, view default.ps1."
   Write-Host ""
   Write-Host "**********************************************************************" -foregroundcolor DarkGreen
}

function Write-Help-Section-Header($description) {
   Write-Host ""
   Write-Host " $description" -foregroundcolor DarkGreen
}

function Write-Help-For-Alias($alias,$description) {
   Write-Host "  > " -nonewline;
   Write-Host "$alias" -foregroundcolor Green -nonewline; 
   Write-Host " = " -nonewline; 
   Write-Host "$description"
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions 
# --------------------------------------------------------------------------------------------------------------
function global:delete_file($file) {
    if($file) { remove-item $file -force -ErrorAction SilentlyContinue | out-null } 
}

function global:delete_directory($directory_name)
{
  rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:get_dacDll(){
    return "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\140\Microsoft.SqlServer.Dac.dll";
}

function global:delete_files($directory_name) {
    Get-ChildItem -Path $directory_name -Include * -File -Recurse | foreach { $_.Delete()}
}

function global:get_vstest_executable($lookin_path) {
    $vstest_exe = Get-ChildItem $lookin_path -Filter Microsoft.TestPlatform* | Select-Object -First 1 | Get-ChildItem -Recurse -Filter vstest.console.exe | % { $_.FullName }
    return $vstest_exe
}

function global:get_version(){
	Write-Host "******************* Getting the Version Number ********************"
	$file_content = Get-Content "./build.properties" -raw
    $file_content = [Regex]::Escape($file_content)
    $file_content = $file_content -replace "(\\r)?\\n", [Environment]::NewLine
    $configuration = ConvertFrom-StringData($file_content)
    return $configuration.'version'
}

function global:get-dotnet(){
	return (Get-Command dotnet).Path
}