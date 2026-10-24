using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using Firebase.Database.Query;

namespace DailyExpense.Views.Admin;

public partial class BcMembersPage : ContentView
{
    private List<BcMember> members = new List<BcMember>();
    private bool isBusy = false;
    public BcMembersPage()
	{
		InitializeComponent();
        _ = LoadAsync();
    }
    private async Task LoadAsync()
    {
        loader.Show();
        try
        {
            var result = await FirebaseService.Db.Child("bc").Child("members").OnceAsync<BcMember>();

            members = result
                .Select(x => { x.Object.Id = x.Key; return x.Object; })
                .OrderBy(x => x.Order)
                .ToList();

            await MainThread.InvokeOnMainThreadAsync(() => cvMembers.ItemsSource = members.ToList());
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Error.\n" + ex.Message, "OK");
        }
        finally
        {
            loader.Hide();
        }
    }
    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (isBusy) return;
        borderName.Stroke = Colors.Gray;

        if (!Session.IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Message", "Only the admin can add members.", "OK");
            return;
        }

        string name = txtName.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            borderName.Stroke = Colors.Red;
            await Application.Current.MainPage.DisplayAlert("Message", "Please enter member name.", "OK");
            txtName.Focus();
            return;
        }

        if (members.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            borderName.Stroke = Colors.Red;
            await Application.Current.MainPage.DisplayAlert("Message", "This Member already exists", "OK");
            txtName.Focus();
            return;
        }

        isBusy = true;
        btnAdd.IsEnabled = false;
        try
        {
            var member = new BcMember
            {
                Name = name,
                Order = (members.Count == 0 ? 0 : members.Max(m => m.Order)) + 1,   // agla number apne aap
                IsActive = true,
                CreatedBy = Preferences.Get("FullName", "Admin"),
                CreatedOn = DateTime.Now
            };

            await FirebaseService.Db.Child("bc").Child("members").PostAsync(member);

            txtName.Text = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Error.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
            btnAdd.IsEnabled = true;
        }
    }
    private async void OnMemberTapped(object sender, TappedEventArgs e)
    {
        var member = (sender as Border)?.BindingContext as BcMember;
        if (member == null || isBusy) return;

        if (!Session.IsAdmin) return;

        string toggleText = member.IsActive ? "Remove (Mark as Inactive)" : "Reactivate (Mark as Active)";

        string choice = await Application.Current.MainPage.DisplayActionSheet(
            member.Name, "Cancel", null, "Change the name", toggleText);

        if (choice == "Change the name")
        {
            string newName = await Application.Current.MainPage.DisplayPromptAsync(
                "Change the name", "Enter new name", "Save", "Cancel", initialValue: member.Name, maxLength: 30);

            newName = newName?.Trim();
            if (string.IsNullOrWhiteSpace(newName) || newName == member.Name) return;

            if (members.Any(m => m.Id != member.Id && m.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                await Application.Current.MainPage.DisplayAlert("Message", "This name already exists.", "OK");
                return;
            }

            member.Name = newName;
            await SaveMemberAsync(member);
        }
        else if (choice == toggleText)
        {
            if (member.IsActive)
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Remove", $"{member.Name} Do you want to remove ? Previous payments will remain safe.", "Yes", "No");
                if (!confirm) return;
            }

            member.IsActive = !member.IsActive;
            await SaveMemberAsync(member);
        }
    }

    private async Task SaveMemberAsync(BcMember member)
    {
        isBusy = true;
        try
        {
            await FirebaseService.Db.Child("bc").Child("members").Child(member.Id).PutAsync(member);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Error.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }
}