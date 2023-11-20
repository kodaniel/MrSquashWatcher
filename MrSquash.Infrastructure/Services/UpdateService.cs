using Squirrel;
using Squirrel.Sources;

namespace MrSquash.Infrastructure.Services;

public class UpdateService
{
    private const string GITHUB_URL = "https://github.com/kodaniel/MrSquashWatcher";
    
    public async Task UpdateApp()
    {
        using (var mgr = new UpdateManager(new GithubSource(GITHUB_URL, "", false)))
        {
            var release = await mgr.UpdateApp();
            if (release != null)
            {
                UpdateManager.RestartApp();
            }
        }
    }
}
