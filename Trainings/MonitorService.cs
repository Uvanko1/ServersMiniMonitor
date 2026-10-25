using System.Diagnostics;

public class MonitorService: IMonitorService
{
    HttpClient httpClient;
    public event FailEvent ServerNotAvailable;

    public MonitorService(HttpClient client)
    {
        httpClient = client;
    }
    async Task<CheckResult> CheckServer(Server server)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(server.Timeout);
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        HttpResponseMessage res;
        try
        {
            res = await httpClient.GetAsync(server.Url, cts.Token);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            return new CheckResult(server.Name, false, stopwatch.ElapsedMilliseconds);
        }
        stopwatch.Stop();
        var time = stopwatch.ElapsedMilliseconds;
        return new CheckResult(server.Name, res.IsSuccessStatusCode, time);
    }

    public async Task<CheckResult[]> CheckAllAsync(List<Server> servers)
    {
        var tasks = servers.Select(server => CheckServer(server));
        var result = await Task.WhenAll(tasks);
        foreach (var res in result)
        {
            if (!res.IsSuccess)
            {
                ServerNotAvailable?.Invoke(res);
            }
        }
        return result;
    }
}