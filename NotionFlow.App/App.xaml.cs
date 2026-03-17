namespace NotionFlow.App;

public partial class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}