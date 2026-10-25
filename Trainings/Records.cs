public record Server
{
    public string Name {get; set;}
    public string Url {get; set;}
    public int Timeout {get; set;}
    public Server(string name, string url, int timeout)
    {
        Name = name;
        Url = url;
        Timeout = timeout;
    }
}

public record CheckResult
{
    public string ServerName {get; set;}
    public bool IsSuccess {get; set;}
    public long TimeToAnswer {get; set;}

    public CheckResult(string serverName, bool isSuccess, long timeToAnswer)
    {
        ServerName = serverName;
        IsSuccess = isSuccess;
        TimeToAnswer = timeToAnswer;
    }
}

public delegate void FailEvent(CheckResult result);