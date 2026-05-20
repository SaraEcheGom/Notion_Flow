namespace NotionFlow.App.Controls;

public partial class TabChip : ContentView
{
    // ── Label ─────────────────────────────────────────────────────────────
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(TabChip), string.Empty);
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── Count ─────────────────────────────────────────────────────────────
    public static readonly BindableProperty CountProperty =
        BindableProperty.Create(nameof(Count), typeof(int), typeof(TabChip), 0);
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    // ── IsActive ──────────────────────────────────────────────────────────
    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(TabChip), false,
            propertyChanged: (b, _, __) => ((TabChip)b).NotifyColors());
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    // ── TapCommand / Parameter ────────────────────────────────────────────
    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(System.Windows.Input.ICommand), typeof(TabChip));
    public System.Windows.Input.ICommand? TapCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(TabChip));
    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    // ── Derivados para colores (leídos desde el XAML) ─────────────────────
    public Style ChipStyle => IsActive
        ? (Style)Application.Current!.Resources["TabChipActive"]
        : (Style)Application.Current!.Resources["TabChipInactive"];

    public Color LabelColor => IsActive ? Colors.White : Color.FromArgb("#5A9A72");
    public Color BadgeBg => IsActive ? Colors.White : Color.FromArgb("#EEF9F3");
    public Color BadgeText => IsActive ? Color.FromArgb("#1A4A32") : Color.FromArgb("#2E8A5E");

    private void NotifyColors()
    {
        OnPropertyChanged(nameof(ChipStyle));
        OnPropertyChanged(nameof(LabelColor));
        OnPropertyChanged(nameof(BadgeBg));
        OnPropertyChanged(nameof(BadgeText));
    }

    public TabChip()
    {
        InitializeComponent();
    }
}