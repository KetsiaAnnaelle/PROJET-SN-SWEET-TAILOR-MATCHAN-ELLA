using System.Globalization;
using System.Collections;

namespace SweetTailor.App.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return $"{d:N0} FCFA".Replace(",", " ");
        if (value is double db)
            return $"{db:N0} FCFA".Replace(",", " ");
        return "0 FCFA";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class DateFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            var format = parameter as string ?? "d MMM yyyy";
            return dt.ToString(format, new CultureInfo("fr-FR"));
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolInverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isTruthy = value switch
        {
            bool boolean => boolean,
            int count => count > 0,
            long count => count > 0,
            ICollection collection => collection.Count > 0,
            string text => !string.IsNullOrEmpty(text),
            _ => false
        };

        return bool.TryParse(parameter?.ToString(), out var invert) && invert
            ? !isTruthy
            : isTruthy;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolean) return Binding.DoNothing;
        return bool.TryParse(parameter?.ToString(), out var invert) && invert
            ? !boolean
            : boolean;
    }
}

public class DueDateColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Color.FromArgb("#E8607F") : Color.FromArgb("#F6F5FA");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public class BooleanHeartFillConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Color.FromArgb("#E8607F") : Colors.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public class BooleanHeartStrokeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Color.FromArgb("#E8607F") : Color.FromArgb("#6B7094");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public class GarmentGeometryConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string asset)
        {
            return Binding.DoNothing;
        }

        var key = asset.ToLowerInvariant() switch
        {
            "gown" => "GeoGown",
            "kaba" => "GeoKaba",
            "suit" => "GeoSuit",
            "skirt" => "GeoSkirt",
            "dress" => "GeoDress",
            _ => throw new InvalidOperationException($"Unknown garment geometry '{asset}'.")
        };

        var resources = Application.Current?.Resources;
        if (resources is not null &&
            resources.TryGetValue(key, out var geometry) &&
            geometry is Microsoft.Maui.Controls.Shapes.Geometry shape)
        {
            return shape;
        }

        throw new InvalidOperationException($"Garment geometry resource '{key}' is missing.");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
