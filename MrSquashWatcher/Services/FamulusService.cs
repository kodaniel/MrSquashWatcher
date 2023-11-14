using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace MrSquashWatcher.Services;

internal class FamulusService : IFamulusService
{
    private HttpClient _client;

    public FamulusService()
    {
        _client = new();
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<IEnumerable<Day>> FetchCurrentWeek()
    {
        var values = new Dictionary<string, string>()
        {
            { "type_id", "2" }
        };

        var url = "https://famulushotel.hu/ajax?do=modules/track_reservation/change_type";
        var content = new FormUrlEncodedContent(values);

        var response = await _client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
            return null;

        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return null;

        return ParseResponse(body);
    }

    public async Task<bool> Reserve(Reservation reservation)
    {
        var values = new Dictionary<string, string>()
        {
            { "track_id", reservation.TrackId.ToString() },
            { "name", reservation.Name },
            { "email", reservation.Email },
            { "phone", reservation.Phone },
            { "comment", reservation.Comment },
            { "start_date", reservation.StartDate.ToString("YYYY-MM-dd") },
            { "start_time", reservation.StartTime.ToString("HH:mm") },
            { "end_time", reservation.EndTime.ToString("HH:mm") },
        };

        var url = "https://famulushotel.hu/ajax?do=modules/track_reservation/save";
        var content = new FormUrlEncodedContent(values);

        //var response = await _client.PostAsync(url, content);
        //if (!response.IsSuccessStatusCode)
        //    return false;

        return true;
    }

    private IEnumerable<Day> ParseResponse(string jsonText)
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
}
