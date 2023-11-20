using MrSquash.Infrastructure.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MrSquash.Infrastructure.Services;

public class FamulusService : IFamulusService
{
    private const int SQUASH_TYPE_ID = 2;
    private HttpClient _client;

    public FamulusService()
    {
        _client = new();
        _client.BaseAddress = new Uri("https://famulushotel.hu/");
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<bool> Reserve(Reservation reservation, CancellationToken cancellationToken = default!)
    {
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

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return false;

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException();

            var result = ParseReserveResponse(body);
            return string.IsNullOrEmpty(result.Error);
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Day>> FetchCurrentWeek(CancellationToken cancellationToken = default!)
    {
        var url = "ajax?do=modules/track_reservation/change_type";
        var values = new Dictionary<string, string>()
        {
            { "type_id", $"{SQUASH_TYPE_ID}" }
        };

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException();

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException();

            return ParseFetchResponse(body);
        }
        catch
        {
            return new List<Day>();
        }
    }

    public async Task<IEnumerable<Day>> FetchNextWeek(Week week, CancellationToken cancellationToken = default!)
    {
        var url = "ajax?do=modules/track_reservation/next_week";
        var values = new Dictionary<string, string>()
        {
            { "type_id", $"{SQUASH_TYPE_ID}" },
            { "date", week.StartDate.ToString("yyyy-MM-dd") }
        };

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException();

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException();

            return ParseFetchResponse(body);
        }
        catch
        {
            return new List<Day>();
        }
    }

    private IEnumerable<Day> ParseFetchResponse(string jsonText)
    {
        JObject jReservations = JObject.Parse(jsonText);
        IList<JToken> jItems = jReservations["items"].Children().ToList();

        IList<Day> dayResults = new List<Day>();
        foreach (JToken result in jItems)
        {
            Day day = result.ToObject<Day>();
            dayResults.Add(day);
        }

        return dayResults;
    }

    private ReserveResponse ParseReserveResponse(string jsonText)
    {
        return JsonConvert.DeserializeObject<ReserveResponse>(jsonText)!;
    }
}
