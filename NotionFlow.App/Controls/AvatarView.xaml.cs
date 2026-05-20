namespace NotionFlow.App.Controls;

public partial class AvatarView : ContentView
{
    // ── Initials ──────────────────────────────────────────────────────────
    public static readonly BindableProperty InitialsProperty =
        BindableProperty.Create(nameof(Initials), typeof(string), typeof(AvatarView), "?");

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    // ── Size ──────────────────────────────────────────────────────────────
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(AvatarView), 42d,
            propertyChanged: (b, _, __) => ((AvatarView)b).NotifyDerived());

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    // ── Derivados (para binding en XAML) ──────────────────────────────────
    public double HalfSize => Size / 2;
    public double FontSz => Size * 0.35;

    private void NotifyDerived()
    {
        OnPropertyChanged(nameof(HalfSize));
        OnPropertyChanged(nameof(FontSz));
    }

    public AvatarView()
    {
        InitializeComponent();
    }
}