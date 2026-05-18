using UIKit;

namespace NotionFlow.App.Platforms.Helpers;

public static partial class SafeAreaHelper
{
    static partial void TryGetTopInset(ref double inset)
    {
        try
        {
            UIWindow? window = null;

            // iOS 13+: buscar la ventana key a través de las scenes
            if (OperatingSystem.IsIOSVersionAtLeast(13, 0))
            {
                foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
                {
                    if (scene is UIWindowScene ws && ws.ActivationState == UISceneActivationState.ForegroundActive)
                    {
                        foreach (var w in ws.Windows)
                        {
                            if (w.IsKeyWindow) { window = w; break; }
                        }
                        if (window is not null) break;
                    }
                }
            }

            // Fallback para versiones antiguas o si no encontramos key window
            window ??= UIApplication.SharedApplication.KeyWindow;

            if (window is not null)
            {
                inset = window.SafeAreaInsets.Top;
            }
            else
            {
                inset = 44;
            }
        }
        catch
        {
            inset = 44;
        }
    }
}