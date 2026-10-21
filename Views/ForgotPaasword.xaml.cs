using DailyExpense.FirebaseClass;
using DailyExpense.MasterView;
using DailyExpense.Models;
using System.Text.RegularExpressions;

namespace DailyExpense.Views;

public partial class ForgotPaasword : ContentPage
{
    private bool isBusy = false;
    public ForgotPaasword()
	{
		InitializeComponent();
	}

    private void Home_Clicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new Login();
    }

    private void OnEyeOldClicked(object sender, EventArgs e)
    {
        txtOldPassword.IsPassword = !txtOldPassword.IsPassword;
        btnEyeOld.Source = txtOldPassword.IsPassword ? "closedeyes.png" : "eye.png";
    }

    private void OnEyeNewClicked(object sender, EventArgs e)
    {
        txtNewPassword.IsPassword = !txtNewPassword.IsPassword;
        btnEyeNew.Source = txtNewPassword.IsPassword ? "closedeyes.png" : "eye.png";
    }

    private void OnEyeConfirmClicked(object sender, EventArgs e)
    {
        txtConfirmPassword.IsPassword = !txtConfirmPassword.IsPassword;
        btnEyeConfirm.Source = txtConfirmPassword.IsPassword ? "closedeyes.png" : "eye.png";
    }

    private void ResetBorders()
    {
        borderregisteredmobilenumber.Stroke = Colors.Gray;
        borderOldPassword.Stroke = Colors.Gray;
        borderNewPassword.Stroke = Colors.Gray;
        borderConfirmPassword.Stroke = Colors.Gray;
    }

    private async void forgotPassword_Clicked(object sender, EventArgs e)
    {
        if (isBusy) return;
        isBusy = true;

        try
        {
            ResetBorders();

            // ---- MOBILE ----
            if (!Regex.IsMatch(txtPhone.Text?.Trim() ?? "", "^\\d{10}$"))
            {
                borderregisteredmobilenumber.Stroke = Colors.Red;
                await DisplayAlert("Message", "Please enter valid 10 digit mobile number.", "OK");
                txtPhone.Focus();
                return;
            }

            // ---- OLD PASSWORD ----
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text))
            {
                borderOldPassword.Stroke = Colors.Red;
                await DisplayAlert("Message", "Please enter your old password.", "OK");
                txtOldPassword.Focus();
                return;
            }

            // ---- NEW PASSWORD STRENGTH ----
            if (!Regex.IsMatch(txtNewPassword.Text ?? "", "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*])[a-zA-Z\\d!@#$%^&*]{8,}$"))
            {
                borderNewPassword.Stroke = Colors.Red;
                await DisplayAlert("Message", "New password must be at least 8 characters with uppercase, lowercase, number and special character.", "OK");
                txtNewPassword.Focus();
                return;
            }

            // ---- CONFIRM MATCH ----
            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                borderConfirmPassword.Stroke = Colors.Red;
                await DisplayAlert("Message", "New password and confirm password do not match.", "OK");
                txtConfirmPassword.Focus();
                return;
            }

            // ---- NEW != OLD ----
            if (txtNewPassword.Text == txtOldPassword.Text)
            {
                borderNewPassword.Stroke = Colors.Red;
                await DisplayAlert("Message", "New password must be different from old password.", "OK");
                txtNewPassword.Focus();
                return;
            }

            // ---- FIREBASE: old password verify + naya password set ----
            string error = await FirebaseService.ChangePassword(
                txtPhone.Text.Trim(), txtOldPassword.Text, txtNewPassword.Text);

            if (error != null)
            {
                borderOldPassword.Stroke = Colors.Red;
                await DisplayAlert("Message", error, "OK");
                txtOldPassword.Focus();
                return;
            }

            // purana session hata do, naye password se login karwao
            Preferences.Clear();

            await DisplayAlert("Success", "Password changed successfully! Please login.", "OK");
            Application.Current.MainPage = new Login();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Something went wrong. Check your internet.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }

    private void login_Clicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new Login();
    }

}