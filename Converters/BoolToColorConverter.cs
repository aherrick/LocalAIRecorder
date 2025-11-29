namespace LocalAIRecorder.Converters;

public class BoolToColorConverter : IValueConverter
{
#pragma warning disable CA1822 // Interface requires instance method
    public object Convert(
        object value,
        Type _,
        object __,
        System.Globalization.CultureInfo ___
    )
    {
        if (value is bool b)
        {
            return b ? Colors.LightBlue : Colors.LightGray;
        }
        return Colors.LightGray;
    }

    public object ConvertBack(
        object _,
        Type __,
        object ___,
        System.Globalization.CultureInfo ____
    )
    {
        throw new NotImplementedException();
    }
#pragma warning restore CA1822
}
