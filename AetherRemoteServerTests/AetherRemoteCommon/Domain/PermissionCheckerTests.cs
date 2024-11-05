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

    [Test]
    public void HasCrossworldLinkshellPermissions_HappyPath()
    {
        var permissions =
            UserPermissions.Speak |
            UserPermissions.CWL1 |
            //UserPermissions.CWL2 |
            UserPermissions.CWL3 |
            UserPermissions.CWL4 |
            //UserPermissions.CWL5 |
            UserPermissions.CWL6 |
            UserPermissions.CWL7 |
            UserPermissions.CWL8;

        Assert.Multiple(() =>
        {
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 1), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 2), Is.False);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 3), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 4), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 5), Is.False);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 6), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 7), Is.True);
            Assert.That(PermissionChecker.HasValidSpeakPermissions(ChatMode.CrossworldLinkshell, permissions, 8), Is.True);
        });
    }
}
