using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;

namespace AetherRemoteServerTests.AetherRemoteCommon.Domain;

public class PermissionCheckerTests
{
    [Test]
    public void HasValidLinkshellPermissions_HappyPath()
    {
        var permissions = UserPermissions.LS1 | UserPermissions.Party | UserPermissions.Speak | UserPermissions.LS6;
        Assert.Multiple(() =>
        {
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 4), Is.False);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.Linkshell, permissions, 2), Is.False);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.Linkshell, permissions, 1), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.Linkshell, permissions, 6), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.Party, permissions), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.Say, permissions), Is.False);
        });
    }
}
