using MAUILLMChatRabbitMQ.ViewModels;

namespace MAUILLMChatRabbitMQ.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel, ConfigPage configPage)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
    }

    private async void OnConfigClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ConfigPage));
    }

    private void OnMessagesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            // Ejecutar en el hilo UI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    ScrollToEnd(false);

                    await Task.Delay(100);
                    ScrollToEnd(true);
                }
                catch { }
            });
        }
    }

    private void ScrollToEnd(bool animate)
    {
        if (_viewModel.Messages.Count > 0)
        {
            var lastItem = _viewModel.Messages.Last();
            MessagesCollectionView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: animate);
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.CleanupAsync();
        _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
    }
}
