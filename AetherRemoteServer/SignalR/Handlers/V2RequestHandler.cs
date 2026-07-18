namespace AetherRemoteServer.SignalR.Handlers;

public class V2RequestHandler(
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    CustomizePlusHandler customizePlusHandler,
    EmoteHandler emoteHandler)
{
    public readonly AddFriendHandler AddFriendHandler = addFriendHandler;
    public readonly BodySwapHandler BodySwapHandler = bodySwapHandler;
    public readonly CustomizePlusHandler CustomizePlusHandler = customizePlusHandler;
    public readonly EmoteHandler EmoteHandler = emoteHandler;
}