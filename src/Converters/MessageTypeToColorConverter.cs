using System.Globalization;
using MAUILLMChatRabbitMQ.Models;

namespace MAUILLMChatRabbitMQ.Converters;

public class MessageTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Sent => Colors.LightBlue,
                MessageType.Received => Colors.LightGreen,
                MessageType.System => Colors.LightGray,
                MessageType.Error => Colors.LightCoral,
                _ => Colors.White
            };
        }
        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
