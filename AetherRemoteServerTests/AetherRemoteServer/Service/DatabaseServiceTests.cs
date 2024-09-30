using AetherRemoteCommon.Domain;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AetherRemoteTests.AetherRemoteServer.Service;

public class DatabaseServiceTests
{
    // Const
    private const string Secret = "SomeSecret";
    private const string FriendCode = "SomeFriendCode";

    // Mocks
    private readonly Mock<ServerConfiguration> config = new(MockBehavior.Strict);
    private readonly ILogger<DatabaseService> logger = Mock.Of<ILogger<DatabaseService>>();

    // Db
    private DatabaseService db;

    [SetUp]
    public void Setup()
    {
        db = new(config.Object, logger);
        db.ClearTables();
    }

    [Test]
    public async Task HappyPath_CreateOrUpdateUser()
    {
        var result = await db.CreateOrUpdateUser(FriendCode, Secret, false).ConfigureAwait(false);
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task HappyPath_GetUser()
    {
        var update = await db.CreateOrUpdateUser(FriendCode, Secret, false).ConfigureAwait(false);
        Assert.That(update, Is.EqualTo(1));

        var result = await db.GetUser(FriendCode).ConfigureAwait(false);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.FriendCode, Is.EqualTo(FriendCode));
            Assert.That(result.Value.Secret, Is.EqualTo(Secret));
            Assert.That(result.Value.IsAdmin, Is.False);
        });
    }

    [Test]
    public async Task HappyPath_CreateOrUpdateUser_Update()
    {
        await db.CreateOrUpdateUser(FriendCode, Secret, false).ConfigureAwait(false);
        await db.CreateOrUpdateUser(FriendCode, Secret, true).ConfigureAwait(false);

        var result = await db.GetUser(FriendCode).ConfigureAwait(false);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.FriendCode, Is.EqualTo(FriendCode));
            Assert.That(result.Value.Secret, Is.EqualTo(Secret));
            Assert.That(result.Value.IsAdmin, Is.True);
        });
    }

    [Test]
    public async Task HappyPath_DeleteUser()
    {
        await db.CreateOrUpdateUser(FriendCode, Secret, false).ConfigureAwait(false);
        var result = await db.GetUser(FriendCode).ConfigureAwait(false);
        Assert.That(result, Is.Not.Null);

        await db.DeleteUser(FriendCode).ConfigureAwait(false);
        var postDeleteResult = await db.GetUser(FriendCode).ConfigureAwait(false);
        Assert.That(postDeleteResult, Is.Null);
    }

    [Test]
    public async Task HappyPath_CreateOrUpdatePermissions()
    {
        await db.CreateOrUpdateUser("FriendCode1", "Secret1", false).ConfigureAwait(false);
        await db.CreateOrUpdateUser("FriendCode2", "Secret2", false).ConfigureAwait(false);

        var (rows, message) = await db.CreateOrUpdatePermissions("FriendCode1", "FriendCode2", UserPermissions.Speak).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(rows, Is.EqualTo(1));
            Assert.That(message, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task HappyPath_GetPermissions()
    {
        await db.CreateOrUpdateUser("FriendCode1", "Secret1", false).ConfigureAwait(false);
        await db.CreateOrUpdateUser("FriendCode2", "Secret2", false).ConfigureAwait(false);

        await db.CreateOrUpdatePermissions("FriendCode1", "FriendCode2", UserPermissions.Speak).ConfigureAwait(false);

        var (permissions, message) = await db.GetPermissions("FriendCode1").ConfigureAwait(false);
        Assert.That(permissions, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(permissions["FriendCode2"], Is.EqualTo(UserPermissions.Speak));
            Assert.That(message, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task HappyPath_CreateOrUpdatePermissions_Update()
    {
        await db.CreateOrUpdateUser("FriendCode1", "Secret1", false).ConfigureAwait(false);
        await db.CreateOrUpdateUser("FriendCode2", "Secret2", false).ConfigureAwait(false);

        await db.CreateOrUpdatePermissions("FriendCode1", "FriendCode2", UserPermissions.Speak).ConfigureAwait(false);
        await db.CreateOrUpdatePermissions("FriendCode1", "FriendCode2", UserPermissions.CWL6).ConfigureAwait(false);

        var (permissions, message) = await db.GetPermissions("FriendCode1").ConfigureAwait(false);
        Assert.That(permissions, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(permissions["FriendCode2"], Is.EqualTo(UserPermissions.CWL6));
            Assert.That(message, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task HappyPath_DeletePermissions()
    {
        await db.CreateOrUpdateUser("FriendCode1", "Secret1", false).ConfigureAwait(false);
        await db.CreateOrUpdateUser("FriendCode2", "Secret2", false).ConfigureAwait(false);

        await db.CreateOrUpdatePermissions("FriendCode1", "FriendCode2", UserPermissions.Speak).ConfigureAwait(false);

        var (permissions, _) = await db.GetPermissions("FriendCode1").ConfigureAwait(false);
        Assert.That(permissions, Has.Count.EqualTo(1));
        Assert.That(permissions["FriendCode2"], Is.EqualTo(UserPermissions.Speak));

        var (rows, _) = await db.DeletePermissions("FriendCode1", "FriendCode2").ConfigureAwait(false);
        Assert.That(rows, Is.EqualTo(1));

        var (deletedPermissions, _) = await db.GetPermissions("FriendCode1").ConfigureAwait(false);
        Assert.That(deletedPermissions, Has.Count.EqualTo(0));
    }
}
