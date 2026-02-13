using System.Globalization;

namespace MAUILLMChatRabbitMQ.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : throw new ArgumentException("Value must be a boolean");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : throw new ArgumentException("Value must be a boolean");
    }
}
