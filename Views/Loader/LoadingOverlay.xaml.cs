namespace DailyExpense.Views.Loader;

public partial class LoadingOverlay : ContentView
{
    private int count = 0;
    public LoadingOverlay()
	{
		InitializeComponent();
	}
    public void Show(string text = "Loading...")
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            count++;
            lblText.Text = text;
            indicator.IsRunning = true;
            IsVisible = true;
        });
    }

    public void Hide()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (count > 0) count--;
            if (count == 0)
            {
                indicator.IsRunning = false;
                IsVisible = false;
            }
        });
    }
}