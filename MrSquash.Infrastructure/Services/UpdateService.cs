using Microsoft.Extensions.Logging;
using Squirrel;
using Squirrel.Sources;

namespace MrSquash.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private const string GITHUB_URL = "https://github.com/kodaniel/MrSquashWatcher";
    private const string ACCESS_TOKEN = "";

    private readonly ILogger<UpdateService> _logger;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
    }

    public async Task UpdateApp()
    {
        using var mgr = new UpdateManager(urlOrPath: null);
        if (mgr.IsInstalledApp)
        {
            try
            {
                using var remoteMgr = new UpdateManager(new GithubSource(GITHUB_URL, ACCESS_TOKEN, false));

                var newVersion = await remoteMgr.UpdateApp();
                if (newVersion != null)
                {
                    UpdateManager.RestartApp();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on updating the application.");
            }
        }
    }
}
