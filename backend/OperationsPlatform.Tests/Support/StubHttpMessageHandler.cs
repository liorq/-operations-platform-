using System.Net;
using System.Text;

namespace OperationsPlatform.Tests.Support;

/// <summary>Stands in for the external weather service, so integration tests never touch the network.</summary>
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly string _json;

    public StubHttpMessageHandler(string json)
    {
        _json = json;
    }

    /// <summary>The addresses the application tried to call, in order.</summary>
    public List<string> RequestedUrls { get; } = new List<string>();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestedUrls.Add(request.RequestUri!.ToString());

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json"),
        };

        return Task.FromResult(response);
    }
}
