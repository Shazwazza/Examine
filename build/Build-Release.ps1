param (
	[Parameter(Mandatory=$false)]
	[int]
	$IsBuildServer = 0
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $RepoRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Release";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}

# Go get nuget.exe if we don't hae it
$NuGet = "$BuildFolder\nuget.exe"
$FileExists = Test-Path $NuGet 
If ($FileExists -eq $False) {
	Write-Host "Retrieving nuget.exe..."
	$SourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	Invoke-WebRequest $SourceNugetExe -OutFile $NuGet
}

if ($IsBuildServer -eq 1) { 
	$MSBuild = "MSBuild.exe"
}
else {
	# ensure we have vswhere
	New-Item "$BuildFolder\vswhere" -type directory -force
	$vswhere = "$BuildFolder\vswhere.exe"
	if (-not (test-path $vswhere))
	{
	   Write-Host "Download VsWhere..."
	   $path = "$BuildFolder\tmp"
	   &$nuget install vswhere -OutputDirectory $path -Verbosity quiet
	   $dir = ls "$path\vswhere.*" | sort -property Name -descending | select -first 1
	   $file = ls -path "$dir" -name vswhere.exe -recurse
	   mv "$dir\$file" $vswhere   
	 }

	$MSBuild = &$vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
	if (-not (test-path $MSBuild)) {
	    throw "MSBuild not found!"
	}
}

Write-Host "MSBUILD = $MSBuild"

# Read XML
$buildXmlFile = (Join-Path $BuildFolder "build.xml")
[xml]$buildXml = Get-Content $buildXmlFile

# Set the version number in SolutionInfo.cs
$SolutionInfoPath = Join-Path -Path $SolutionRoot -ChildPath "SolutionInfo.cs"

# Set the copyright
$Copyright = "Copyright " + [char]0x00A9 + " Shannon Deminick " + (Get-Date).year
(gc -Path $SolutionInfoPath) `
	-replace "(?<=AssemblyCopyright\(`").*(?=`"\))", $Copyright |
	Set-Content -Path $SolutionInfoPath -Encoding UTF8

$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "Examine.sln"

#restore nuget packages
Write-Host "Restoring nuget packages..."
& $NuGet restore $SolutionPath

# Iterate projects and update their versions
[System.Xml.XmlElement] $root = $buildXml.get_DocumentElement()
[System.Xml.XmlElement] $project = $null
foreach($project in $root.ChildNodes) {

	$projectPath = Join-Path -Path $SolutionRoot -ChildPath $project.id
	$projectAssemblyInfo = Join-Path -Path $projectPath -ChildPath "Properties\AssemblyInfo.cs"

	if (Test-Path -Path $projectAssemblyInfo){
		$projectVersion = $project.version.Split("-")[0];

		Write-Host "Updating verion for $projectPath to $($project.version) ($projectVersion)"

		#update assembly infos with correct version

		(gc -Path $projectAssemblyInfo) `
			-replace "(?<=Version\(`")[.\d]*(?=`"\))", "$projectVersion.0" |
			Set-Content -Path $projectAssemblyInfo -Encoding UTF8

		(gc -Path $projectAssemblyInfo) `
			-replace "(?<=AssemblyInformationalVersion\(`")[.\w-]*(?=`"\))", $project.version |
			Set-Content -Path $projectAssemblyInfo -Encoding UTF8
	}
}

# Build the solution in release mode
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Rebuild
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

# Iterate projects and output them
foreach($project in $root.ChildNodes) {	
	$nuSpec = Join-Path -Path $SolutionRoot -ChildPath "$($project.id)\$($project.id).nuspec";	
	& $NuGet pack $nuSpec -OutputDirectory $ReleaseFolder -Version $project.version -Properties copyright=$Copyright
}


""
"Build is done!"