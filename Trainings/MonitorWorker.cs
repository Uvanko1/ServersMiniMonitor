public class MonitorWorker: BackgroundService
{
    private readonly IMonitorService monitorService;
    private readonly ILogger<MonitorWorker> Logger;
    private readonly List<Server> ServersList;

    public MonitorWorker(IMonitorService monitor_service, ILogger<MonitorWorker> logger, List<Server> servers)
    {
        monitorService = monitor_service;
        ServersList = servers;
        Logger = logger;
        monitor_service.ServerNotAvailable += OnServerNotAvailable;
    }
    protected void OnServerNotAvailable(CheckResult result)
    {
        Logger.LogWarning($"Сервер {result.ServerName} не отвечает");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await monitorService.CheckAllAsync(ServersList);
            foreach (var res in result)
            {
                if (res.IsSuccess)
                {
                    Logger.LogInformation($"{res.ServerName} -> OK -> {res.TimeToAnswer}");
                }
            }
            Logger.LogInformation($"Всего проверок: {result.Count()}");
            var successes = result.Where(res => res.IsSuccess == true);
            Logger.LogInformation($"Успешных: {successes.Count()}");
            Logger.LogWarning($"Ошибок: {result.Where(res => res.IsSuccess == false).Count()}");
            if (successes.Any())
            {
                Logger.LogInformation($"Среднее время ответа: {successes.Select(res => res.TimeToAnswer).Average()}");
            }
            else
            {
                Logger.LogWarning("Ни один сервер не ответил");
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}