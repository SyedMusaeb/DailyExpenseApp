using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using System.Text.RegularExpressions;

namespace DailyExpense.Views;


public partial class Register : ContentPage
{
    private bool isBusy = false;
    public Register()
	{
		InitializeComponent();
	}

    private void login_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }


    private async void register_Clicked(object sender, EventArgs e)
    {
        if (isBusy) return;
        isBusy = true;

        try
        {
            // ---- VALIDATION ----
            if (string.IsNullOrWhiteSpace(txtname.Text) ||
                string.IsNullOrWhiteSpace(txtphone.Text) ||
                string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                await DisplayAlert("Message", "Please fill all required fields.", "OK");
                return;
            }

            if (!Regex.IsMatch(txtphone.Text.Trim(), "^\\d{10}$"))
            {
                await DisplayAlert("Message", "Please enter valid mobile number.", "OK");
                txtphone.Focus();
                return;
            }

            if (!Regex.IsMatch(txtpassword.Text, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*])[a-zA-Z\\d!@#$%^&*]{8,}$"))
            {
                await DisplayAlert("Message", "Please enter strong password (8+ characters, uppercase, lowercase, number, special character).", "OK");
                txtpassword.Focus();
                return;
            }

            if (txtpassword.Text != txtconfirmpassword.Text)
            {
                await DisplayAlert("Message", "Password does not match.", "OK");
                txtconfirmpassword.Focus();
                return;
            }
            SetLoading(true);

            // ---- FIREBASE REGISTER ----
            await FirebaseService.Register(txtname.Text.Trim(), txtphone.Text.Trim(), txtpassword.Text);

            // register ke baad Firebase automatically login kar deta hai, to logout karo
            FirebaseService.Logout();

            await DisplayAlert("Message", "Account created successfully. Please login.", "OK");
            App.Current.MainPage = new Login();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("EmailExists") || ex.Message.Contains("EMAIL_EXISTS"))
                await DisplayAlert("Message", "This mobile number is already registered.", "OK");
            else
                await DisplayAlert("Error", "Registration failed. Check your internet.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
            SetLoading(false);
        }
    }
    private void Home_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Homepage();
    }

    private void OnEyeClicked(object sender, EventArgs e)
    {
        txtpassword.IsPassword = !txtpassword.IsPassword;

        // icon bhi change kar do taaki user ko pata chale
        if (txtpassword.IsPassword)
            btnEye.Source = "closedeyes.png";   // password chupa hua
        else
            btnEye.Source = "eye.png";    // password dikh raha
    }

    private void OnEyeConfirmClicked(object sender, EventArgs e)
    {
        txtconfirmpassword.IsPassword = !txtconfirmpassword.IsPassword;

        // icon bhi change kar do taaki user ko pata chale
        if (txtconfirmpassword.IsPassword)
            btnEyeconfirm.Source = "closedeyes.png";   // password chupa hua
        else
            btnEyeconfirm.Source = "eye.png";    // password dikh raha
    }

    private void SetLoading(bool isLoading)
    {
        loader.IsVisible = isLoading;
        loader.IsRunning = isLoading;

        login.IsEnabled = !isLoading;
        login.Text = isLoading ? "Please wait..." : "Submit";
    }


}