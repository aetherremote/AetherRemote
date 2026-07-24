namespace AetherRemoteServer.SignalR.Handlers;

public class AggregateRequestHandler(
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    CustomizePlusHandler customizePlusHandler,
    EmoteHandler emoteHandler,
    HonorificHandler honorificHandler,
    HypnosisHandler hypnosisHandler,
    HypnosisStopHandler hypnosisStopHandler,
    InitializeSessionHandler initializeSessionHandler,
    MoodlesHandler moodlesHandler,
    OnlineNotificationHandler onlineNotificationHandler,
    RemoveFriendHandler removeFriendHandler,
    SpeakHandler speakHandler,
    TerminateSessionHandler terminateSessionHandler,
    TransformationHandler transformationHandler,
    TwinningHandler twinningHandler,
    UpdateFriendHandler updateFriendHandler,
    UpdateGlobalPermissionsHandler updateGlobalPermissionsHandlerHandler)
{
    public readonly AddFriendHandler AddFriendHandler = addFriendHandler;
    public readonly BodySwapHandler BodySwapHandler = bodySwapHandler;
    public readonly CustomizePlusHandler CustomizePlusHandler = customizePlusHandler;
    public readonly EmoteHandler EmoteHandler = emoteHandler;
    public readonly HonorificHandler HonorificHandler = honorificHandler;
    public readonly HypnosisHandler HypnosisHandler = hypnosisHandler;
    public readonly HypnosisStopHandler HypnosisStopHandler = hypnosisStopHandler;
    public readonly InitializeSessionHandler InitializeSessionHandler = initializeSessionHandler;
    public readonly MoodlesHandler MoodlesHandler = moodlesHandler;
    public readonly OnlineNotificationHandler OnlineNotificationHandler = onlineNotificationHandler;
    public readonly RemoveFriendHandler RemoveFriendHandler = removeFriendHandler;
    public readonly SpeakHandler SpeakHandler = speakHandler;
    public readonly TerminateSessionHandler TerminateSessionHandler = terminateSessionHandler;
    public readonly TransformationHandler TransformationHandler = transformationHandler;
    public readonly TwinningHandler TwinningHandler = twinningHandler;
    public readonly UpdateFriendHandler UpdateFriendHandler = updateFriendHandler;
    public readonly UpdateGlobalPermissionsHandler UpdateGlobalPermissionsHandlerHandler = updateGlobalPermissionsHandlerHandler;
}