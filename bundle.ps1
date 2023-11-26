param(
	[Parameter(Mandatory = $true)]
	[string]$tag,
	[string]$repo = "https://github.com/kodaniel/MrSquashWatcher",
	[string]$squirrelVersion = "2.11.1"
)

# build / publish your app
dotnet publish -c Release -o ".\publish" 

# find Squirrel.exe path and add an alias
Set-Alias Squirrel ($env:USERPROFILE + "\.nuget\packages\clowd.squirrel\" + $squirrelVersion + "\tools\Squirrel.exe");
Write-Host "Using tool: " + Squirrel -ForegroundColor Magenta

# download currently live version
Write-Host "Pulling latest version from GitHub: " + $repo -ForegroundColor Magenta
Squirrel github-down --repoUrl $repo

# build new version and delta updates.
Write-Host "Creating package" -ForegroundColor Magenta
Squirrel pack `
	--framework net8,vcredist143-x64 `
	--packId "MrSquashWatcher" `
	--packVersion $tag `
	--packDir ".\publish"