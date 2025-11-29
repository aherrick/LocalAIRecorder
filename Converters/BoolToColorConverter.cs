namespace LocalAIRecorder.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    )
    {
        if (value is bool b)
        {
            return b ? Colors.LightBlue : Colors.LightGray;
        }
        return Colors.LightGray;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
