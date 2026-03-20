using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace NexusMods.App.UI.Controls.LoadingSection;

/// <summary>
/// A container control that overlays a spinner on its content when <see cref="IsActive"/> is true.
/// Bind <see cref="IsActive"/> to the ViewModel's <c>IsLoading</c> property.
/// </summary>
public partial class LoadingSection : UserControl
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<LoadingSection, bool>(nameof(IsActive));

    public static readonly StyledProperty<object?> SectionContentProperty =
        AvaloniaProperty.Register<LoadingSection, object?>(nameof(SectionContent));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<LoadingSection, string?>(nameof(Message));

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    [Content]
    public object? SectionContent
    {
        get => GetValue(SectionContentProperty);
        set => SetValue(SectionContentProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingSection()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            LoadingOverlay.IsVisible = change.GetNewValue<bool>();
        }
        else if (change.Property == MessageProperty)
        {
            var text = change.GetNewValue<string?>();
            LoadingText.Text = text ?? string.Empty;
            LoadingText.IsVisible = !string.IsNullOrEmpty(text);
        }
    }
}
