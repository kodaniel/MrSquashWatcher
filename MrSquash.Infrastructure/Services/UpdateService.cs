using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace MrSquash.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private const string GITHUB_URL = "https://github.com/kodaniel/MrSquashWatcher";
    private const string ACCESS_TOKEN = ""; // If empty, only limited number of requests are allowed

    private readonly ILogger<UpdateService> _logger;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
    }

    public async Task UpdateApp()
    {
        var githubSrc = new GithubSource(GITHUB_URL, ACCESS_TOKEN, false);
        var mgr = new UpdateManager(githubSrc);

        if (mgr.IsInstalled)
        {
            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return; // no update available

            // download new version
            await mgr.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
    }
}
