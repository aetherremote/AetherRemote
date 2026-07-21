namespace AetherRemoteServer.SignalR.Handlers;

public class AggregateRequestHandler(
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    CustomizePlusHandler customizePlusHandler,
    EmoteHandler emoteHandler,
    GetAccountDataHandler getAccountDataHandler,
    HonorificHandler honorificHandler,
    HypnosisHandler hypnosisHandler,
    HypnosisStopHandler hypnosisStopHandler,
    MoodlesHandler moodlesHandler,
    OnlineStatusUpdateHandler onlineStatusUpdateHandler,
    RemoveFriendHandler removeFriendHandler,
    SpeakHandler speakHandler,
    TransformationHandler transformationHandler,
    TwinningHandler twinningHandler,
    UpdateFriendHandler updateFriendHandler,
    UpdateGlobalPermissionsHandler updateGlobalPermissionsHandlerHandler)
{
    public readonly AddFriendHandler AddFriendHandler = addFriendHandler;
    public readonly BodySwapHandler BodySwapHandler = bodySwapHandler;
    public readonly CustomizePlusHandler CustomizePlusHandler = customizePlusHandler;
    public readonly EmoteHandler EmoteHandler = emoteHandler;
    public readonly GetAccountDataHandler GetAccountDataHandler = getAccountDataHandler;
    public readonly HonorificHandler HonorificHandler = honorificHandler;
    public readonly HypnosisHandler HypnosisHandler = hypnosisHandler;
    public readonly HypnosisStopHandler HypnosisStopHandler = hypnosisStopHandler;
    public readonly MoodlesHandler MoodlesHandler = moodlesHandler;
    public readonly OnlineStatusUpdateHandler OnlineStatusUpdateHandler = onlineStatusUpdateHandler;
    public readonly RemoveFriendHandler RemoveFriendHandler = removeFriendHandler;
    public readonly SpeakHandler SpeakHandler = speakHandler;
    public readonly TransformationHandler TransformationHandler = transformationHandler;
    public readonly TwinningHandler TwinningHandler = twinningHandler;
    public readonly UpdateFriendHandler UpdateFriendHandler = updateFriendHandler;
    public readonly UpdateGlobalPermissionsHandler UpdateGlobalPermissionsHandlerHandler = updateGlobalPermissionsHandlerHandler;
}