param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d+?\.\d+?\.\d+?\.\d")]
	[string]
	$ReleaseVersionNumber
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}

# Set the version number in SolutionInfo.cs
$SolutionInfoPath = Join-Path -Path $SolutionRoot -ChildPath "SolutionInfo.cs"
(gc -Path $SolutionInfoPath) `
	-replace "(?<=Version\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $SolutionInfoPath -Encoding UTF8

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "Examine.sln"
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$CoreExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "Examine";
$WebExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "ExamineWebDemo";
$ExamineAzureFolder = Join-Path -Path $ReleaseFolder -ChildPath "Examine.Azure";

New-Item $CoreExamineFolder -Type directory
New-Item $WebExamineFolder -Type directory
New-Item $ExamineAzureFolder -Type directory

$include = @('*Examine*.dll','*Lucene*.dll','ICSharpCode.SharpZipLib.dll')
$CoreExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine\bin\Release";
Copy-Item "$CoreExamineBinFolder\*.dll" -Destination $CoreExamineFolder -Include $include

$include = @('*Examine*.dll','*Lucene*.dll', '*Azure*.dll','ICSharpCode.SharpZipLib.dll')
$ExamineAzureBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine.Azure\bin\Release";
Copy-Item "$ExamineAzureBinFolder\*.dll" -Destination $ExamineAzureFolder -Include $include

$ExamineWebDemoFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine.Web.Demo";
Copy-Item "$ExamineWebDemoFolder\*" -Destination $WebExamineFolder -Recurse
$IndexSet = Join-Path $WebExamineFolder -ChildPath "App_Data\SimpleIndexSet2";
$include = @('*.sdf','SimpleIndexSet2*')
Remove-Item $IndexSet -Recurse
$SqlCeDb = Join-Path $WebExamineFolder -ChildPath "App_Data\Database1.sdf";
Remove-Item $SqlCeDb 

$CoreNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "Examine.nuspec";
Copy-Item $CoreNuSpecSource -Destination $CoreExamineFolder
# Copy-Item "$BuildFolder\nuget-transforms\Core\web.config.transform" -Destination (New-Item (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms") -Type directory);

$CoreNuSpec = Join-Path -Path $CoreExamineFolder -ChildPath "Examine.nuspec";

$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe" 
& $NuGet pack $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber

""
"Build $ReleaseVersionNumber is done!"
"NuGet packages also created, so if you want to push them just run:"
"  nuget push $CoreNuSpec"