param(
	[Parameter(Mandatory = $true)]
	[string]$tag
)

dotnet restore
dotnet build -c Release --no-restore
dotnet test --no-build -c Release

# build / publish your app
dotnet publish -c Release -r win-x64 --self-contained -o ".\publish" 

# build new version and delta updates.
Write-Host "Creating package" -ForegroundColor Magenta
vpk pack `
	--framework net8,vcredist143-x64 `
	--packId "MrSquashWatcher" `
	--packVersion $tag `
	--packDir ".\publish" `
	--mainExe "Mr Squash Watcher.exe" `
	--skipVeloAppCheck
