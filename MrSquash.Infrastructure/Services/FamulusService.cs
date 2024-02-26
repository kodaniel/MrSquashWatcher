using Microsoft.Extensions.Logging;
using MrSquash.Infrastructure.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;

namespace MrSquash.Infrastructure.Services;

public class FamulusService : IFamulusService
{
    private const int SQUASH_TYPE_ID = 2;

    private readonly ILogger<FamulusService> _logger;
    private HttpClient _client;

    public FamulusService(ILogger<FamulusService> logger)
    {
        _logger = logger;

        _client = new();
        _client.BaseAddress = new Uri("https://famulushotel.hu/");
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _client.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<bool> Reserve(Reservation reservation, CancellationToken cancellationToken = default!)
    {
        var attempt = 0;
        var url = "ajax?do=modules/track_reservation/save";
        var values = new Dictionary<string, string>()
        {
            { "track_id", reservation.TrackId.ToString() },
            { "name", reservation.Name! },
            { "email", reservation.Email! },
            { "phone", reservation.Phone! },
            { "comment", reservation.Comment! },
            { "price", reservation.Price.ToString() },
            { "start_date", reservation.StartDate.ToString("yyyy-MM-dd") },
            { "start_time", reservation.StartTime.ToString("HH:mm") },
            { "end_time", reservation.EndTime.ToString("HH:mm") },
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(3),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry to reserve. Attempts: {attempt}", ++attempt);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await pipeline.ExecuteAsync(static async (state, token) => await state.client.PostAsync(state.url, state.content, token),
                (content, url, client: _client), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Status code is not 200.");

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException("Response body is empty.");

            var result = ParseReserveResponse(body);
            return string.IsNullOrEmpty(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve the game.");
            return false;
        }
    }

    public async Task<IEnumerable<Day>> FetchCurrentWeek(CancellationToken cancellationToken = default!)
    {
        var attempt = 0;
        var url = "ajax?do=modules/track_reservation/change_type";
        var values = new Dictionary<string, string>()
        {
            { "type_id", $"{SQUASH_TYPE_ID}" }
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(3),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry to fetch the current week. Attempts: {attempt}", ++attempt);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await pipeline.ExecuteAsync(static async (state, token) => await state.client.PostAsync(state.url, state.content, token),
                (content, url, client: _client), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Status code is not 200.");

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException("Response body is empty.");

            return ParseFetchResponse(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch the current week.");
            return Enumerable.Empty<Day>();
        }
    }

    public async Task<IEnumerable<Day>> FetchNextWeek(Week week, CancellationToken cancellationToken = default!)
    {
        var attempt = 0;
        var url = "ajax?do=modules/track_reservation/next_week";
        var values = new Dictionary<string, string>()
        {
            { "type_id", $"{SQUASH_TYPE_ID}" },
            { "date", week.StartDate.ToString("yyyy-MM-dd") }
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(3),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry to fetch the next week. Attempts: {attempt}", ++attempt);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await pipeline.ExecuteAsync(static async (state, token) => await state.client.PostAsync(state.url, state.content, token),
                (content, url, client: _client), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Status code is not 200.");

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException("Response body is empty.");

            return ParseFetchResponse(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch the next week.");
            return Enumerable.Empty<Day>();
        }
    }

    private IEnumerable<Day> ParseFetchResponse(string jsonText)
    {
        JObject jReservations = JObject.Parse(jsonText);
        IList<JToken> jItems = jReservations["items"]?.Children().ToList() ?? new List<JToken>();

        IList<Day> dayResults = new List<Day>();
        foreach (JToken result in jItems)
        {
            Day? day = result.ToObject<Day>();
            if (day == null)
                continue;

            dayResults.Add(day);
        }

        return dayResults;
    }

    private ReserveResponse ParseReserveResponse(string jsonText)
    {
        return JsonConvert.DeserializeObject<ReserveResponse>(jsonText)!;
    }
}
