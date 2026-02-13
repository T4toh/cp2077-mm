using System.Globalization;
using Avalonia.Data.Converters;

namespace NexusMods.App.UI.Converters;

public class EnumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string paramString)
            return false;

        var currentStatus = value.ToString();
        var allowedStatuses = paramString.Split('|');

        return allowedStatuses.Any(s => string.Equals(s, currentStatus, StringComparison.OrdinalIgnoreCase));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
