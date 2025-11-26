using LocalAIRecorder.ViewModels;

namespace LocalAIRecorder;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(DetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

public class BoolToColorConverter : IValueConverter
{
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
        object value,
        Type _,
        object __,
        System.Globalization.CultureInfo ___
    )
    {
        throw new NotImplementedException();
    }
}