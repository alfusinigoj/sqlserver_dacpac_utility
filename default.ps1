$script:project_config = "Debug"

properties {
  $base_dir = resolve-path .
  $publish_dir = "$base_dir\publish-artifacts"
  $solution_file = "$base_dir\$solution_name.sln"
  $project_file = "$base_dir\src\$solution_name\$solution_name.csproj"
  $test_dir = "$base_dir\test"
  $nuget = "nuget.exe"
  $msbuild = Get-LatestMsbuildLocation
  $vstest = get_vstest_executable
  $date = Get-Date 
  $version = get_version
  $release_id = "win-x64"
  $platform = "Any CPU"
}

#These are aliases for other build tasks. They typically are named after the camelcase letters (rd = Rebuild Databases)
task default -depends DevBuild
task cib -depends CiBuild
task cip -depends CiPublish
task dp -depends DevPublish
task ? -depends help

task emitProperties {
  Write-Host "solution_name=$solution_name"
  Write-Host "build_dir=$build_dir"
  Write-Host "solution_file=$solution_file"
  Write-Host "test_dir=$test_dir"
  Write-Host "publish_dir=$publish_dir"
  Write-Host "project_config=$project_config"
  Write-Host "version=$version"
}

task help {
   Write-Help-Header
   Write-Help-Section-Header "Comprehensive Building"
   Write-Help-For-Alias "(default)" "Intended for first build or when you want a fresh, clean local copy"
   Write-Help-For-Alias "cip" "Continuous Integration build (long and thorough) with publishing"
   Write-Help-For-Alias "dp" "Publish in developer machine, locally"
   Write-Help-Footer
   exit 0
}

#These are the actual build tasks. They should be Pascal case by convention
task DevBuild -depends WriteVersion, SetDebugBuild, emitProperties, Clean, Restore, Compile, UnitTest
task DevPublish -depends DevBuild, Publish
task CiBuild -depends WriteVersion, SetReleaseBuild, emitProperties, Clean, Restore, Compile, UnitTest
task CiPublish -depends CiBuild, Publish

task SetDebugBuild {
    $script:project_config = "Debug"
}

task SetReleaseBuild {
    $script:project_config = "Release"
}

task WriteVersion {
    echo $version > .version
}

task Clean {
    Write-Host "******************* Now cleaning the solution and artifacts *********************"
    if (Test-Path $publish_dir) {
        delete_directory $publish_dir
    }

    Get-ChildItem .\ -include obj,bin -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse}

    exec { 
        & $msbuild /t:clean /v:m /p:Configuration=$project_config $solution_file 
    }
    if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
}

task Restore {
    Write-Host "******************* Now restoring the solution dependencies *********************"
    exec { 
        & $msbuild /t:restore $solution_file /v:m /p:NuGetInteractive="true" /p:RuntimeIdentifier=$release_id
        if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
    }
}

task Compile -depends Restore {
    Write-Host "******************* Now compiling the solution *********************"
    exec { 
        & $msbuild /t:build /v:m /p:Configuration=$project_config /nologo /p:Platform=$platform /nologo $solution_file 
    }
    if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
}

task UnitTest -depends Compile{
    Write-Host "******************* Now running unit tests *********************"
    Push-Location $base_dir
    $test_assemblies = @((Get-ChildItem -Recurse -Filter "*Tests.dll" | Where-Object {$_.Directory -like '*bin*'} | Where-Object {$_.Directory -notlike '*ref*'}).FullName) -join ' '
    Write-Host "Executing tests on the following assemblies: $test_assemblies"
    Start-Process -FilePath $vstest -ArgumentList $test_assemblies ,"/Parallel" -NoNewWindow -Wait
    Pop-Location
    if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
 }

 task Publish -depends Clean, Restore {
    Write-Host "******************* Now publishing the project $project_file *********************"
    exec { 
        & $msbuild /t:publish /v:m /p:PublishTrimmed="true" /p:PublishReadyToRun="false" /p:PublishSingleFile="true" /p:Platform=$platform /p:SelfContained="true" /p:RuntimeIdentifier=$release_id /p:PublishDir=$publish_dir /p:Configuration=$project_config /nologo /nologo $project_file 
    }
    if($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}
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

function global:delete_files($directory_name) {
    Get-ChildItem -Path $directory_name -Include * -File -Recurse | foreach { $_.Delete()}
}

function global:get_vstest_executable() {
    $vstest_exe = exec { & "c:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\vswhere.exe"  -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -property installationPath}
    $vstest_exe = join-path $vstest_exe 'Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe'
    return $vstest_exe
}

function global:get_version() {
    $gitversion = "$base_dir\tools\gitversion\gitversion.exe"
    return exec { & $gitversion /output json /showvariable FullSemVer }
}

