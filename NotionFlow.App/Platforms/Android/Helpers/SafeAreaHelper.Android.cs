using Android.App;
using AndroidX.Core.View;
 
namespace NotionFlow.App.Platforms.Helpers;
 
public static partial class SafeAreaHelper
{
    static partial void TryGetTopInset(ref double inset)
    {
        try
        {
            var activity = Platform.CurrentActivity ?? (Activity?)null;
            if (activity?.Window?.DecorView?.RootWindowInsets is null)
            {
                inset = FallbackStatusBarHeight(activity);
                return;
            }
 
            // API moderna: WindowInsetsCompat para compatibilidad amplia (API 21+)
            var insetsCompat = ViewCompat.GetRootWindowInsets(activity.Window.DecorView);
            if (insetsCompat is not null)
            {
                var systemBars = insetsCompat.GetInsets(WindowInsetsCompat.Type.SystemBars());
                if (systemBars is not null)
                {
                    // Convertir pixeles a dp dividiendo por la densidad
                    double density = activity.Resources?.DisplayMetrics?.Density ?? 1.0;
                    inset = systemBars.Top / density;
                    return;
                }
            }
 
            inset = FallbackStatusBarHeight(activity);
        }
        catch
        {
            // En caso de cualquier excepción, usamos un valor seguro por defecto
            inset = 24;
        }
    }
 
    /// <summary>
    /// Fallback: lee la altura del status bar desde el recurso del sistema.
    /// Usado si WindowInsets aún no está disponible (ej. antes de que la view esté attached).
    /// </summary>
    static double FallbackStatusBarHeight(Activity? activity)
    {
        try
        {
            if (activity?.Resources is null) return 24;
 
            int resourceId = activity.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                int pixels = activity.Resources.GetDimensionPixelSize(resourceId);
                double density = activity.Resources.DisplayMetrics?.Density ?? 1.0;
                return pixels / density;
            }
        }
        catch { }
        return 24; // valor por defecto razonable
    }
}
 