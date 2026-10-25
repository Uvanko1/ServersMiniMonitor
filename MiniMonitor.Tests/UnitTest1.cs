using System.Net;
using System.Net.Security;
using System.Reflection;
using Xunit;

namespace MiniMonitor.Tests;

public class UnitTest1
{
    [Fact]
    public async Task TestGetAsyncOk()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var handler = new FakeHttpHandler(response, null, 100);

        var httpClient = new HttpClient(handler);

        var result = await httpClient.GetAsync("https://google.com");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task TestMonitorServiceOk()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500)
        };

        var handler = new FakeHttpHandler(response, null, 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.True(result[0].IsSuccess);
    }

    [Fact]
    public async Task TestMonitorServiceInternalError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500)
        };

        var handler = new FakeHttpHandler(response, null, 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.False(result[0].IsSuccess);
    }

    [Fact]
    public async Task TestMonitorServiceException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500)
        };

        var handler = new FakeHttpHandler(response, new Exception("Произошла ошибка"), 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.False(result[0].IsSuccess);
    }

    [Fact]
    public async Task TestMonitorServiceEvent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500)
        };

        var handler = new FakeHttpHandler(response, new Exception("Произошла ошибка"), 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);
        bool eventWasCalled = false;
        monitorService.ServerNotAvailable += res =>
        {
            eventWasCalled = true;
        };
        var result = await monitorService.CheckAllAsync(servers);

        Assert.True(eventWasCalled);
    }

    [Fact]
    public async Task TestMonitorService3Results()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500),
            new Server("yandex", "https://ya.ru", 1000),
            new Server("github", "https://github.com", 2000)
        };

        var handler = new FakeHttpHandler(response, null, 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.Equal(3, result.Length);
    }

    [Fact]
    public async Task TestMonitorServiceResultContain()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500),
            new Server("yandex", "https://ya.ru", 1000),
            new Server("github", "https://github.com", 2000)
        };

        var handler = new FakeHttpHandler(response, null, 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.Contains("google", result.Select(server => server.ServerName));
        Assert.Contains("yandex", result.Select(server => server.ServerName));
        Assert.Contains("github", result.Select(server => server.ServerName));
    }

    [Fact]
    public async Task TestMonitorServiceEventRes()
    {
        CheckResult? eventResult = null;
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500)
        };

        var handler = new FakeHttpHandler(response, new Exception("Произошла ошибка"), 100);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);
        bool eventWasCalled = false;
        monitorService.ServerNotAvailable += res =>
        {
            eventWasCalled = true;
            eventResult = res;
        };
        var result = await monitorService.CheckAllAsync(servers);

        Assert.True(eventWasCalled);
        Assert.Equal("google", eventResult?.ServerName);
        Assert.False(eventResult?.IsSuccess);
    }

    [Fact]
    public async Task TestFakeHandlerThrowsConfiguredException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var exception = new HttpRequestException("Нет соединения");

        var handler = new FakeHttpHandler(response, exception, 100);
        var httpClient = new HttpClient(handler);
        await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.GetAsync("https://google.com"));
    }

    [Fact]
    public async Task TestMonitorServiceDelay()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var servers = new List<Server>
        {
            new Server("google", "https://google.com", 500),
            new Server("yandex", "https://ya.ru", 700)
        };

        var handler = new FakeHttpHandler(response, null, 600);

        var httpClient = new HttpClient(handler);

        var monitorService = new MonitorService(httpClient);

        var result = await monitorService.CheckAllAsync(servers);

        Assert.False(result[0].IsSuccess);
        Assert.True(result[1].IsSuccess);
    }
}
