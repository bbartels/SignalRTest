using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/", async (IHubContext<TestHub> context, ILogger<Program> logger, HttpRequest request) =>
{
    logger.LogInformation("Request made");
    var channel = Channel.CreateUnbounded<int>();
    var resultReader = await context.Clients.Client(TestHub.ConnectionId).InvokeAsync<ChannelReader<int>>("SendRequest", channel.Reader, CancellationToken.None);
    await channel.Writer.WriteAsync(1);
    await channel.Writer.WriteAsync(1);
    await channel.Writer.WriteAsync(1);
    channel.Writer.Complete();
    while (await resultReader.WaitToReadAsync())
    {
        while (resultReader.TryRead(out var count))
        {
            logger.LogInformation($"{count}");
        }
    }

});
app.MapHub<TestHub>("/hubs/test");

app.Run();

public class TestHub(ILogger<TestHub> logger) : Hub
{
    public static string ConnectionId = null!;

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        ConnectionId = Context.ConnectionId;
        logger.LogInformation($"{ConnectionId} connected");
    }
}
