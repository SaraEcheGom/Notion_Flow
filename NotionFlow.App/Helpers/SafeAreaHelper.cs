namespace NotionFlow.App.Platforms.Helpers;

/// <summary>
/// Devuelve el inset superior (status bar / notch) en unidades independientes de densidad.
/// Implementación parcial por plataforma:
///   - Android: lee WindowInsets.SystemBars top.
///   - iOS / MacCatalyst: lee Window.SafeAreaInsets.Top.
///   - Windows: devuelve 0 (no aplica).
/// </summary>
public static partial class SafeAreaHelper
{
    /// <summary>
    /// Obtiene el inset superior del sistema en unidades MAUI (dp).
    /// </summary>
    public static double GetTopInset()
    {
        double inset = 0;
        TryGetTopInset(ref inset);
        return inset;
    }

    // Implementado en archivos por plataforma.
    static partial void TryGetTopInset(ref double inset);
}