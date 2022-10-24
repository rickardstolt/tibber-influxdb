
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Net.Http.Headers;
using System.Reflection;
using Tibber.Sdk;

Log("Starting tibber-influxdb...");

var influxdbHost = Environment.GetEnvironmentVariable("INFLUXDB_HOST") ?? "127.0.0.1";
var influxdbPort = Environment.GetEnvironmentVariable("INFLUXDB_PORT") ?? "8086";
var influxdbToken = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN") ?? "";
var influxdbOrg = Environment.GetEnvironmentVariable("INFLUXDB_ORG") ?? "";
var influxdbBucket = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET") ?? "tibber";

var tibberToken = Environment.GetEnvironmentVariable("TIBBER_TOKEN") ?? "";
var tibberHome = Environment.GetEnvironmentVariable("TIBBER_HOME") ?? "";

var period = int.Parse(Environment.GetEnvironmentVariable("PERIOD") ?? "3600");
var maxEntries = int.Parse(Environment.GetEnvironmentVariable("MAXENTRIES") ?? "168");

Log("Using:");
Log($"INFLUXDB_HOST={influxdbHost}");
Log($"INFLUXDB_PORT={influxdbPort}");
Log($"INFLUXDB_TOKEN={(influxdbToken.Length > 0 ? "******" : "")}");
Log($"INFLUXDB_ORG={influxdbOrg}");
Log($"INFLUXDB_BUCKET={influxdbBucket}");
Log($"TIBBER_TOKEN={(tibberToken.Length > 0 ? "******" : "")}");
Log($"TIBBER_HOME={tibberHome}");
Log($"PERIOD={period}");
Log($"MAXENTRIES={maxEntries}");

do
{
    var nextRun = DateTime.UtcNow.AddSeconds(period);

    try
    {
        Log($"Fetching consumption for the {maxEntries} last hours from Tibber...");

        var userAgent = new ProductInfoHeaderValue("tibber-influxdb", Assembly.GetExecutingAssembly().GetName().Version?.ToString());

        var client = new TibberApiClient(tibberToken, userAgent);

        var basicData = await client.GetBasicData();

        var homeId = basicData.Data.Viewer.Homes.FirstOrDefault(home => home.Id?.ToString()?.ToLower() == tibberHome.ToLower())?.Id
            ?? basicData.Data.Viewer.Homes.First()?.Id;

        var consumption = await client.GetHomeConsumption(homeId.Value, EnergyResolution.Hourly, maxEntries);

        using var influxDBClient = InfluxDBClientFactory.Create($"http://{influxdbHost}:{influxdbPort}", influxdbToken);

        var writeApiAsync = influxDBClient.GetWriteApiAsync();

        Log($"Writing {consumption.Count} data points to Influxdb...");

        foreach (var item in consumption)
        {
            var point = PointData.Measurement("consumption")
            .Tag("home", homeId.Value.ToString())
            .Field("consumption", item.Consumption)
            .Field("consumptionUnit", item.ConsumptionUnit)
            .Field("cost", item.Cost)
            .Field("currency", item.Currency)
            .Field("unitPrice", item.UnitPrice)
            .Field("unitPriceVat", item.UnitPriceVat)
            .Timestamp(item.From.Value, WritePrecision.Ns);

            await writeApiAsync.WritePointAsync(point, influxdbBucket, influxdbOrg);
        }

        Log($"Done!");
    }
    catch (Exception e)
    {
        Log($"Error: ({e.GetType().FullName}) {e.Message}");
    }

    Log($"Next run sheduled to {nextRun.ToLocalTime()}");
    await Task.Delay(nextRun - DateTime.UtcNow);

} while (period > 0);

static void Log(string msg)
{
    Console.WriteLine(msg);
}