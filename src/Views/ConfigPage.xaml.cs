using MAUILLMChatRabbitMQ.ViewModels;

namespace MAUILLMChatRabbitMQ.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage(ConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
