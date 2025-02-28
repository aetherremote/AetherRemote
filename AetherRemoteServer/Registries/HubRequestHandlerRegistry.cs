using AetherRemoteServer.Handlers;

namespace AetherRemoteServer.Registries;

/// <summary>
///     Bundles all the handles into a single registry
/// </summary>
public class HubRequestHandlerRegistry(
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    EmoteHandler emoteHandler,
    GetAccountDataHandler getAccountDataHandler,
    RemoveFriendHandler removeFriendHandler,
    SpeakHandler speakHandler,
    TransformHandler transformHandler,
    TwinningHandler twinningHandler,
    UpdateFriendHandler updateFriendHandler)
{
    public AddFriendHandler AddFriendHandler { get; } = addFriendHandler;
    public BodySwapHandler BodySwapHandler { get; } = bodySwapHandler;
    public EmoteHandler EmoteHandler { get; } = emoteHandler;
    public GetAccountDataHandler GetAccountDataHandler { get; } = getAccountDataHandler;
    public RemoveFriendHandler RemoveFriendHandler { get; } = removeFriendHandler;
    public SpeakHandler SpeakHandler { get; } = speakHandler;
    public TransformHandler TransformHandler { get; } = transformHandler;
    public TwinningHandler TwinningHandler { get; } = twinningHandler;
    public UpdateFriendHandler UpdateFriendHandler { get; } = updateFriendHandler;
}