param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("^\d+\.\d+\.(?:\d+\.\d+$|\d+$)")]
	[string]
	$ReleaseVersionNumber,
	[Parameter(Mandatory=$true)]
	[string]
	[AllowEmptyString()]
	$PreReleaseName
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$ProgFiles86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)");
$MSBuild = "$ProgFiles86\MSBuild\14.0\Bin\MSBuild.exe"

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber$PreReleaseName";
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
(gc -Path $SolutionInfoPath) `
	-replace "(?<=AssemblyInformationalVersion\(`")[.\w-]*(?=`"\))", "$ReleaseVersionNumber$PreReleaseName" |
	sc -Path $SolutionInfoPath -Encoding UTF8
# Set the copyright
$Copyright = "Copyright � Shannon Deminick " + (Get-Date).year
(gc -Path $SolutionInfoPath) `
	-replace "(?<=AssemblyCopyright\(`").*(?=`"\))", $Copyright |
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

New-Item $CoreExamineFolder -Type directory
New-Item $WebExamineFolder -Type directory

$include = @('*Examine*.dll','*Examine*.pdb','*Lucene*.dll','ICSharpCode.SharpZipLib.dll')
$CoreExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine\bin\Release";
Copy-Item "$CoreExamineBinFolder\*.*" -Destination $CoreExamineFolder -Include $include

#$ExamineWebDemoFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine.Web.Demo";
#Copy-Item "$ExamineWebDemoFolder\*" -Destination $WebExamineFolder -Recurse
#$IndexSet = Join-Path $WebExamineFolder -ChildPath "App_Data\SimpleIndexSet2";
#$include = @('*.sdf','SimpleIndexSet2*')
#Remove-Item $IndexSet -Recurse
#$SqlCeDb = Join-Path $WebExamineFolder -ChildPath "App_Data\Database1.sdf";
#Remove-Item $SqlCeDb 

$CoreNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "Nuspecs\Examine\*";
Copy-Item $CoreNuSpecSource -Destination $CoreExamineFolder
$CoreNuSpec = Join-Path -Path $CoreExamineFolder -ChildPath "Examine.nuspec";
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe" 
& $NuGet pack $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName -Properties copyright=$Copyright


""
"Build $ReleaseVersionNumber$PreReleaseName is done!"