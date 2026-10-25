public interface IMonitorService
{
    Task<CheckResult[]> CheckAllAsync(List<Server> servers);

    public event FailEvent ServerNotAvailable;
}