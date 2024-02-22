param(
	[Parameter(Mandatory = $true)]
	[string]$tag
)

# build / publish your app
dotnet publish -c Release -o ".\publish" 

# build new version and delta updates.
Write-Host "Creating package" -ForegroundColor Magenta
vpk pack `
	--framework net8,vcredist143-x64 `
	--packId "MrSquashWatcher" `
	--packVersion $tag `
	--packDir ".\publish" `
	--mainExe "Mr Squash Watcher.exe" `
	--skipVeloAppCheck
