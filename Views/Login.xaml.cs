using DailyExpense.FirebaseClass;
using DailyExpense.MasterView;
using DailyExpense.Models;
using Firebase.Database.Query;
using System.Text.RegularExpressions;

namespace DailyExpense.Views;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
        //MakeAdmin();
    }

    private async void login_Clicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn != null) btn.IsEnabled = false;

        try
        {
            ResetBorders();

            if (string.IsNullOrWhiteSpace(txtphoneusername.Text))
            {
                borderusername.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter your mobile number.", "OK");
                txtphoneusername.Focus();
                return;
            }

            // Firebase me phone se hi login hota hai, isliye 10 digit check
            if (!Regex.IsMatch(txtphoneusername.Text.Trim(), "^\\d{10}$"))
            {
                borderusername.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter valid 10 digit mobile number.", "OK");
                txtphoneusername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                borderpassword.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter your password.", "OK");
                txtpassword.Focus();
                return;
            }

            // ---- FIREBASE LOGIN ----
            bool ok = await FirebaseService.Login(txtphoneusername.Text.Trim(), txtpassword.Text);

            if (!ok)
            {
                borderpassword.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message",
                    "Login Failed! Mobile number & password does not match.", "OK");
                return;
            }

            // ---- PROFILE LAO (naam, role, status) ----
            var user = await FirebaseService.Db
                .Child("users")
                .Child(FirebaseService.Uid)
                .OnceSingleAsync<NewRegistration>();

            if (user == null || user.Status != "Active")
            {
                FirebaseService.Logout();
                await Application.Current.MainPage.DisplayAlert("Message", "Your account is not active.", "OK");
                return;
            }

            // ---- SESSION SAVE ----
            Preferences.Set("UserId", FirebaseService.Uid);
            Preferences.Set("FullName", user.FullName);
            Preferences.Set("Role", user.UserType ?? "User");
            Preferences.Set("IsLoggedIn", true);

            App.Current.MainPage = new MasterPage();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Something went wrong. Please check your internet.\n" + ex.Message, "OK");
        }
        finally
        {
            if (btn != null) btn.IsEnabled = true;
        }
    }

    private void register_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Register();
    }

    private void forgot_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new ForgotPaasword();
    }

    private void OnEyeClicked(object sender, EventArgs e)
    {
        txtpassword.IsPassword = !txtpassword.IsPassword;
        if (txtpassword.IsPassword)
        {
            btnEye.Source = "closedeyes.png";   
        }

        else
        {
            btnEye.Source = "eye.png";    
        }
            
    }
    private void ResetBorders()
    {
        borderusername.Stroke = Colors.Gray;
        borderpassword.Stroke = Colors.Gray;
    }

}