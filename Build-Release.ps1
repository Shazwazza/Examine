param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d+?\.\d+?\.\d+?\.\d")]
	[string]
	$ReleaseVersionNumber
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$Is64BitSystem = (Get-WmiObject -Class Win32_OperatingSystem).OsArchitecture -eq "64-bit"
$Is64BitProcess = [IntPtr]::Size -eq 8

$RegistryArchitecturePath = ""
if ($Is64BitProcess) { $RegistryArchitecturePath = "\Wow6432Node" }

$FrameworkArchitecturePath = ""
if ($Is64BitSystem) { $FrameworkArchitecturePath = "64" }

$ClrVersion = (Get-ItemProperty -path "HKLM:\SOFTWARE$RegistryArchitecturePath\Microsoft\VisualStudio\10.0")."CLR Version"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework$FrameworkArchitecturePath\$ClrVersion\MSBuild.exe"

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
$UmbExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExamine";
$UmbExaminePDFFolder = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExaminePDF";
$WebExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "ExamineWebDemo";

New-Item $CoreExamineFolder -Type directory
New-Item $UmbExamineFolder -Type directory
New-Item $UmbExaminePDFFolder -Type directory
New-Item $WebExamineFolder -Type directory

$include = @('*Examine*.dll','*Lucene*.dll')
$CoreExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Examine\bin\Release";
Copy-Item "$CoreExamineBinFolder\*.dll" -Destination $CoreExamineFolder -Include $include

$UmbExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "UmbracoExamine\bin\Release";
Copy-Item "$UmbExamineBinFolder\*.dll" -Destination $UmbExamineFolder -Include $include

$include = @('UmbracoExamine.PDF.dll','itextsharp.dll')
$UmbExaminePDFBinFolder = Join-Path -Path $SolutionRoot -ChildPath "UmbracoExamine.PDF\bin\Release";
Copy-Item "$UmbExaminePDFBinFolder\*.dll" -Destination $UmbExaminePDFFolder -Include $include

$ExamineWebDemoFolder = Join-Path -Path $SolutionRoot -ChildPath "Examine.Web.Demo";
Copy-Item "$ExamineWebDemoFolder\*" -Destination $WebExamineFolder -Recurse
$IndexSet = Join-Path $WebExamineFolder -ChildPath "App_Data\SimpleIndexSet2";
$include = @('*.sdf','SimpleIndexSet2*')
Remove-Item $IndexSet -Recurse
$SqlCeDb = Join-Path $WebExamineFolder -ChildPath "App_Data\Database1.sdf";
Remove-Item $SqlCeDb 

""
"Build $ReleaseVersionNumber is done!"