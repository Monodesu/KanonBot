using System.Net.WebSockets;
// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");
// int a = 0;
// while (true)
// {
//     Console.WriteLine("hello1");
//     await Task.Delay(100);
// }

using KanonBot.Message;
using KanonBot.Config;
using KanonBot.WebSocket;
using KanonBot.Drivers;
using Tomlyn;
using Websocket.Client;

// Console.WriteLine("Init");
var ExitEvent = new ManualResetEvent(false);
var c = new Chain().msg("hello").image("C:\\hello.png", Image.Type.file).msg("test\nhaha");
c.append(new RawMessage("Test"));
Console.WriteLine(c);

var configPath = "config.toml";
if (File.Exists(configPath))
{
    Config.inner = Config.load(configPath);
}
else
{
    Config.inner = Config.Default();
    Config.inner.save(configPath);
}

var config = Config.inner!;

Console.WriteLine(config);

var drivers = new Drivers();
drivers.append(
    new CQ.Driver($"ws://{config.cqhttp?.host}:{config.cqhttp?.port}")
    .onMessage((client, msg) =>
    {
        Console.WriteLine(msg);
        // client.Send("xxxxx");
    })
    .onEvent((client, e) =>
    {
        Console.WriteLine(e);
        // client.Send("xxxxx");
    })
);
drivers.StartAll();
ExitEvent.WaitOne();
