namespace NotionFlow.App.Controls;

public partial class PageHeaderComponent : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(PageHeaderComponent),
            defaultValue: string.Empty,
            propertyChanged: OnTitleChanged);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(PageHeaderComponent),
            defaultValue: string.Empty,
            propertyChanged: OnSubtitleChanged);

    public static readonly BindableProperty ShowNotificationProperty =
        BindableProperty.Create(
            nameof(ShowNotification),
            typeof(bool),
            typeof(PageHeaderComponent),
            defaultValue: true,
            propertyChanged: OnShowNotificationChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool ShowNotification
    {
        get => (bool)GetValue(ShowNotificationProperty);
        set => SetValue(ShowNotificationProperty, value);
    }

    public PageHeaderComponent()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Binding handles UI updates automatically
    }

    private static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Binding handles UI updates automatically
    }

    private static void OnShowNotificationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Binding handles UI updates automatically
    }
}
