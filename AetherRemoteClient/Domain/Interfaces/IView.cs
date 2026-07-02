namespace AetherRemoteClient.Domain.Interfaces;

public interface IView
{
    /// <summary>
    ///     The enum this view represents
    /// </summary>
    View View { get; }
    
    /// <summary>
    ///     Draw the content of the view
    /// </summary>
    public void Draw();

    /// <summary>
    ///     Initialize any Ui subscriptions, if required
    /// </summary>
    public void Initialize()
    {
    }
}