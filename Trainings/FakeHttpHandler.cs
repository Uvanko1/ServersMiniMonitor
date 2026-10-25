public class FakeHttpHandler: HttpMessageHandler
{
    private readonly HttpResponseMessage response;
    private readonly Exception? exception;

    private readonly int delay;

    public FakeHttpHandler(HttpResponseMessage httpResponse, Exception? ex, int del)
    {
        response = httpResponse;
        exception = ex;
        delay = del;
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken){
        if (exception != null)
        {
            throw exception;
        }
        else
        {
            await Task.Delay(delay, cancellationToken);
            return response;
        }
    }
}