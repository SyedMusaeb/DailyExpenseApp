using DailyExpense.Views;

namespace DailyExpense;

public partial class Homepage : ContentPage
{
	public Homepage()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {

        //DisplayAlert("Message","Hello!! I am working on it...", "Ok");
        App.Current.MainPage = new Login();
    }
}