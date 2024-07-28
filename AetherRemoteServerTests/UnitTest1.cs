using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;
using AetherRemoteCommon.Domain.Network.Login;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace AetherRemoteServerTests;

public class Tests
{
    private Process serverExe;

    private const string ConnectionUrl = "http://10.0.0.148:25565/mainHub";
    private HubConnection connection;

    private const string ValidSecret = "test";
    private const string InvalidSecret = "test2";

    [SetUp]
    public void Setup()
    {
        serverExe = Process.Start(@"C:\Users\Mora\Desktop\Remote\AetherRemote\AetherRemoteServer\bin\Debug\net7.0\AetherRemoteServer.exe");
        connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();
        connection.StartAsync().Wait();
    }

    [TearDown]
    public void Teardown()
    {
        serverExe.Dispose();
        connection.DisposeAsync();
    }

    [Test]
    public void TestConnection()
    {
        Assert.That(connection.State, Is.EqualTo(HubConnectionState.Connected));
    }

    [Test]
    public async Task TestLogin()
    {
        var request = new LoginRequest(ValidSecret);
        var response = await connection.InvokeAsync<LoginResponse>(Constants.ApiLogin, request);
        Assert.Multiple(() =>
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task TestLoginBadSecret()
    {
        var request = new LoginRequest(InvalidSecret);
        var response = await connection.InvokeAsync<LoginResponse>(Constants.ApiLogin, request);
        Assert.Multiple(() =>
        {
            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo("Secret doesn't exist"));
        });
    }

    [Test]
    public async Task TestCreateOrUpdateFriend()
    {
        var request = new CreateOrUpdateFriendRequest(ValidSecret, new Friend(""));
        var response = await connection.InvokeAsync<CreateOrUpdateFriendResponse>(Constants.ApiCreateOrUpdateFriend, request);
        Assert.That(response.Success, Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        connection.StopAsync().Wait();
        serverExe.Kill();
    }
}
