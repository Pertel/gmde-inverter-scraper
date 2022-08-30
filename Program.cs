using System.Text;
using System.Text.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            System.Console.WriteLine("Usage: <action (read|pull)> <filepath> [gmdeSiteId] [username] [password]");
            System.Console.WriteLine("username, password and gmdeSiteId is only required for the pull action");
            System.Console.WriteLine("gmdeSiteId is an integer that gmde generates for your site. It's visible in the URL when you have selected the site. eg. 'http://portal.global-mde.com/dz/User/overview/1234' where gmdeSiteId would be 1234");
            return;
        }
        var action = args[0];
        var filePath = args[1];

        switch (action)
        {
            case "read":
                if (!File.Exists(filePath))
                {
                    throw new InvalidOperationException($"file {filePath} does not exist");
                }
                await ReadExistingDataFromFile(filePath);
                break;
            case "pull":
                if (File.Exists(filePath))
                {
                    throw new InvalidOperationException($"file {filePath} already exist");
                }
                if (args.Length < 4) {
                    System.Console.WriteLine("write action requires username, password and gmdeSiteId");
                    return;
                }
                var siteId = args[2];
                var userName = args[3];
                var password = args[4];
                await PullDataAndSaveInFile(filePath, userName, password, siteId);
                
                break;
            default:
                throw new ArgumentOutOfRangeException("action should be 'read' or 'pull'");
        }


    }

    private static async Task ReadExistingDataFromFile(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);

        var totalSavedInPeriod = 0f;

        foreach (var line in lines)
        {
            var parts = line.Split(";");
            var date = parts[0];
            var data = JsonSerializer.Deserialize<GmdeDto>(parts[1], new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var savedEnergy = data.Data.DataPoints.Sum(dp => dp.LoadConsumption - dp.GridSupply);
            var totalSaved = savedEnergy / 12 / 1000;

            var totalPvGeneration = data.Data.DataPoints.Sum(dp => dp.PvGeneration);
            var totalGeneration = totalPvGeneration / 12 / 1000; // divide by 12 because each point is for 5 minutes. there are 12 five-minute intervals to an hour

            Console.WriteLine($"The {date}, we saved: {totalSaved} kWh");
            totalSavedInPeriod += totalSaved;
        }
        System.Console.WriteLine($"Total saved: {totalSavedInPeriod} kWh" );
    }

    private static async Task PullDataAndSaveInFile(string filePath, string userName, string password, string siteId)
    {
        var client = new HttpClient();
        string sessionId = await GetSession(client);

        client.DefaultRequestHeaders.Add("ASP.NET_SessionId", sessionId);

        await Login(userName, password, client);

        var endDate = DateTime.Now;
        var startDate = endDate.AddMonths(-1);
        StringBuilder data = await PullDataForPeriod(client, startDate, endDate, siteId);

        File.Delete(filePath);
        File.WriteAllText(filePath, data.ToString());
    }

    private static async Task<StringBuilder> PullDataForPeriod(HttpClient client, DateTime startDate, DateTime endDate, string siteId)
    {
        var getDataUrl = "http://portal.global-mde.com/dz/user/overviewdata";

        var data = new System.Text.StringBuilder();

        for (var curDate = startDate; curDate.Date <= endDate.Date; curDate = curDate.AddDays(1))
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{getDataUrl}/{siteId}?date={curDate.ToString("yyyy-MM-dd")}"),
            };
            request.Headers.Add("Accept", @"application/json, text/javascript, */*; q=0.01");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            data.AppendLine($"{curDate.ToString("yyyy-MM-dd")};{content}");
            System.Console.WriteLine($"Added content for {curDate.ToShortDateString()}");
        }

        return data;
    }

    private static async Task Login(string userName, string password, HttpClient client)
    {
        var loginUrl = "http://portal.global-mde.com/dz/home/login";

        var formValues = new Dictionary<string, string> {
            { "username", userName},
            {"password", password},
            {"ValidateCode", "False"},
            {"url", "/user/clientIndex"}
        };

        HttpContent formContent = new FormUrlEncodedContent(formValues);

        var loginRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(loginUrl),
            Content = formContent
        };

        var loginResult = await client.SendAsync(loginRequest);
        System.Console.WriteLine($"login statuscode: {loginResult.StatusCode}");
    }

    private static async Task<string> GetSession(HttpClient client)
    {
        var frontPageRequest = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri("http://portal.global-mde.com/") };
        frontPageRequest.Headers.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        frontPageRequest.Headers.Add("User-Agent", @"Mozilla/5.0 (X11; Linux x86_64; rv:103.0) Gecko/20100101 Firefox/103.0");

        var frontPageResult = await client.SendAsync(frontPageRequest);

        System.Console.WriteLine($"frontpage status: {frontPageResult.StatusCode}");
        var cookieHeaders = frontPageResult.Headers.Where(h => h.Key.Equals("Set-Cookie"));

        var cookies = cookieHeaders.SelectMany(ch => ch.Value);
        var sessionCookie = cookies.Single(c => c.StartsWith("ASP.NET_SessionId"));

        var sessionId = sessionCookie.Split(";").First().Split("=")[1];
        System.Console.WriteLine($"SessionId: {sessionId}");
        return sessionId;
    }
}


