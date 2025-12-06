using System;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController
{
    public void Debug()
    {
        try
        {
            // Test Code Here
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"{e}");
        }
    }
}