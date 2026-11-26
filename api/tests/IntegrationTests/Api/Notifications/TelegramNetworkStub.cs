using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace IntegrationTests.Api.Notifications;

/// <summary>
/// The network under the real Telegram client: it records every request url and answers like the Bot API, so a test
/// sees exactly the address the client builds (the production bug of #24 lived there).
/// </summary>
internal sealed class TelegramNetworkStub : HttpMessageHandler
{
    public ConcurrentQueue<Uri> Requests { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Enqueue(request.RequestUri!);
        var method = request.RequestUri!.AbsolutePath.Split('/')[^1];
        var result = method switch
        {
            "getMe" => """{"id":1,"is_bot":true,"username":"home_real_bot"}""",
            "getWebhookInfo" => """{"url":""}""",
            "sendMessage" => """{"message_id":1}""",
            _ => "true",
        };
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""{"ok":true,"result":{{result}}}""", Encoding.UTF8, "application/json"),
        });
    }
}
