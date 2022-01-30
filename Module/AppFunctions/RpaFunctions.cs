using Bygdrift.CsvTools;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Module.AppFunctions
{
    public class RpaFunctions
    {
        //public readonly AppBase<Settings> App;

        //public RpaFunctions(ILogger<RpaFunctions> logger) => App = new AppBase<Settings>(logger);

        //[FunctionName(nameof(RpaGetDaluxConnectionTimeAsync))]
        //public async Task RpaGetDaluxConnectionTimeAsync([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        //{
        //    if (App.Settings.RpaHostName == null || !App.Settings.RpaGetDaluxConnectionTime)
        //        return;

        //    var csv = new Csv("Date, Seconds, Status, Note");
        //    using var client = new HttpClient();
        //    client.BaseAddress = new Uri(App.Settings.RpaHostName);
        //    var json = BuildJson(App);
        //    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync("/api/rpa/daluxFM/getLoginTime", content);
        //    double seconds = 120;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var res = await response.Content.ReadAsStringAsync();
        //        _ = double.TryParse(res, NumberStyles.Any, CultureInfo.InvariantCulture, out seconds);
        //    }
        //    csv.AddRow(App.LoadedLocal, seconds, response.StatusCode, response.ReasonPhrase);
        //    App.Mssql.InserCsv(csv, "ConnectionTime", false, false);
        //}

        public static JObject BuildJson(AppBase<Settings> app)
        {
            return new JObject{
                {
                    "login", new JObject {
                        { "customer", app.Settings.DaluxCustomerId },
                        { "user", app.Settings.DaluxUser },
                        { "password", app.Settings.DaluxPassword },
                    }
                },
                {"url", "https://fm.dalux.com" }
            };
        }
    }
}