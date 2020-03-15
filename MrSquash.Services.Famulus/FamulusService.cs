using HtmlAgilityPack;
using MrSquash.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MrSquash.Services.Famulus
{
    public class FamulusService
    {
        public async Task<IEnumerable<Day>> LoadDays(string url)
        {
            try
            {
                var html = await Task.Run(() =>
                {
                    var web = new HtmlWeb();
                    return web.LoadFromWebAsync(url);
                });

                return ParseReservationScript(html);
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<Day> ParseReservationScript(HtmlDocument htmlDoc)
        {
            Match m = null;
            var scriptTags = htmlDoc.DocumentNode.SelectNodes("//script");
            var script = scriptTags.Cast<HtmlNode>().FirstOrDefault(x =>
            {
                m = Regex.Match(x.InnerText, @"(reservation\.initalize\()(.*)(\);)");
                return m.Success;
            });

            if (script is null)
                return null;

            var jsonText = m.Groups[2];
            return ParseJson(jsonText.Value);
        }

        private IEnumerable<Day> ParseJson(string jsonText)
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
}
