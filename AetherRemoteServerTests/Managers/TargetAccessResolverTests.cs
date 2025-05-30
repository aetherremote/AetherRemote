using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.V2;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Handlers.Helpers;
using Moq;

namespace AetherRemoteServerTests.Managers;

public class TargetAccessResolverTests
{
    private const string Sender = "senderFriendCode";
    private const string Target = "targetFriendCode";

    private const LinkshellPermissions DefaultLinkshellPermissions = 
        LinkshellPermissions.Ls1 | 
        LinkshellPermissions.Ls7 |
        LinkshellPermissions.Cwl1;
    
    private const PrimaryPermissions DefaultPrimaryPermissions = 
        PrimaryPermissions.Speak | 
        PrimaryPermissions.BodySwap;
    
    private Mock<IDatabaseService> _database;
    private Mock<IClientConnectionService> _connections;
    private TargetAccessResolver _manager;
    
    [SetUp]
    public void Setup()
    {
        _database = new Mock<IDatabaseService>();
        _connections = new Mock<IClientConnectionService>();
        _manager = new TargetAccessResolver(_connections.Object, _database.Object);
        
        _database
            .Setup(c => c.GetPermissions(Target))
            .Returns(Task.FromResult(new FriendPermissions
            {
                Permissions = new Dictionary<string, UserPermissions>
                {
                    {
                        Sender, new UserPermissions
                        {
                            Primary = DefaultPrimaryPermissions,
                            Linkshell = DefaultLinkshellPermissions,
                        }
                    }
                }
            }));

        _connections
            .Setup(c => c.TryGetClient(Target))
            .Returns(new ClientInfo("SOME_ID"));
    }

    [Test]
    public async Task TestPrimaryPermissions()
    {
        var result = await _manager.TryGetAuthorizedConnectionAsync(Sender, Target, PrimaryPermissions.Speak);
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(AetherRemoteActionErrorCode.Success));
            Assert.That(result.Value, Is.EqualTo("SOME_ID"));
        });
    }
    
    [Test]
    public async Task TestPrimaryPermissions_NoPermissions()
    {
        var result = await _manager.TryGetAuthorizedConnectionAsync(Sender, Target, PrimaryPermissions.Moodles);
        Assert.That(result.Result, Is.EqualTo(AetherRemoteActionErrorCode.TargetHasNotGrantedSenderPermissions));
    }
    
    [Test]
    public async Task TestLinkshellPermissions()
    {
        var result = await _manager.TryGetAuthorizedConnectionAsync(Sender, Target, LinkshellPermissions.Cwl1);
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(AetherRemoteActionErrorCode.Success));
            Assert.That(result.Value, Is.EqualTo("SOME_ID"));
        });
    }
}