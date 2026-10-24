using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using DailyExpense.Views;
using DailyExpense.Views.Admin;
using Microsoft.Maui.Controls;

namespace DailyExpense.MasterView;

public partial class MasterPage : ContentPage
{
    private Stack<View> navigationHistory = new Stack<View>();
    private DateTime lastBackPress = DateTime.MinValue;

    protected override bool OnBackButtonPressed()
    {
        // 1. pichla page maujood hai to ek step peeche jao (← arrow jaisa hi)
        if (navigationHistory.Count > 0)
        {
            OnBackClicked(null, null);
            return true;            // true = hamne handle kar liya, app band mat karo
        }

        // 2. pehli screen par hain: 2 second ke andar dobara back dabane par hi app band ho
        if ((DateTime.Now - lastBackPress).TotalSeconds < 2)
            return false;           // false = system app band kar de

        lastBackPress = DateTime.Now;

            #if ANDROID
                    Android.Widget.Toast.MakeText(
                        Android.App.Application.Context,
                        "Please press back button to exit",
                        Android.Widget.ToastLength.Short).Show();
            #endif

        return true;
    }
    public MasterPage()
	{
		InitializeComponent();
        LoadUserSession();
        //PageContent.Content = new DailyExpenseForm();
        //ShowExpenseForm();
        

        lblMenu.IsVisible = Session.IsAdmin;
        lblMenu.IsVisible = true;

        if (Session.IsAdmin)
            ShowAdminPage();
        else
            ShowExpenseForm();

    }

    public View PageBody
    {
        get => PageContent.Content;
        set => PageContent.Content = value;
    }


    private void Home_Clicked(object sender, EventArgs e)
    {
        //PageContent.Content = new DailyExpenseForm();
        navigationHistory.Clear();
        imgBack.IsVisible = false;
        ShowExpenseForm();
    }

    private void LoadUserSession()
    {
        string fullName = Preferences.Get("FullName", "Guest");
        string userId = Session.UserId;

        lblWelcome.Text = $"Welcome, {fullName}!";
    }
  
    private void ShowExpenseForm()
    {
        var form = new DailyExpenseForm();

        form.GoToSamanList += () =>
        {
            ShowSamanList();
        };

        //PageContent.Content = form;
        NavigateTo(form, addToHistory: false);
    }

    private void ShowSamanList()
    {
        var samanListPage = new SamanListPage();

        //samanListPage.OnCategorySelected += (categoryName, selectedMonth) =>
        //{
        //    NavigateTo(new CategoryDetailPage(categoryName, selectedMonth));
        //};

        //NavigateTo(samanListPage);

        samanListPage.OnCategorySelected += (categoryName, selectedMonth) =>
        {
            var detailPage = new CategoryDetailPage(categoryName, selectedMonth);

            
            detailPage.OnEditRequested += (expenseId) =>
            {
                var editPage = new EditExpensePage(expenseId);

                editPage.OnSaved += () =>
                {
                    
                    OnBackClicked(null, null);
                    if (PageContent.Content is CategoryDetailPage cdp)
                        cdp.Refresh();
                };

                NavigateTo(editPage);
            };

            NavigateTo(detailPage);
        };

        NavigateTo(samanListPage);
    }

    private void NavigateTo(View newPage, bool addToHistory = true)
    {
        if (addToHistory && PageContent.Content != null)
        {
            navigationHistory.Push(PageContent.Content);
        }

        PageContent.Content = newPage;
        imgBack.IsVisible = navigationHistory.Count > 0;
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        if (navigationHistory.Count > 0)
        {
            var previousPage = navigationHistory.Pop();
            PageContent.Content = previousPage;
            imgBack.IsVisible = navigationHistory.Count > 0;

            if (previousPage is SamanListPage samanList)
                samanList.Refresh();
            else if (previousPage is CategoryDetailPage detail)
                detail.Refresh();
            else if (previousPage is AdminPage admin)
                admin.Refresh();
        }


    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");

        if (confirm)
        {
            FirebaseService.Logout();     // ye line add karo
            Preferences.Clear();
            Application.Current.MainPage = new Login();
        }
    }


    private async void OnMenuClicked(object sender, EventArgs e)
    {
        var options = new List<string> { "Add Expense", "Expense List", "BC Sheet" };
        if (Session.IsAdmin)
        {
            options.Add("BC Members");
            options.Add("Admin Panel");
        }


        string choice = await DisplayActionSheet("Menu", "Cancel", null,
            options.ToArray());

        switch (choice)
        {
            case "Add Expense":
                navigationHistory.Clear();
                imgBack.IsVisible = false;
                ShowExpenseForm();
                break;

            case "Expense List":
                navigationHistory.Clear();
                imgBack.IsVisible = false;
                ShowSamanList();
                break;

            case "Admin Panel":
                navigationHistory.Clear();
                imgBack.IsVisible = false;
                ShowAdminPage();
                break;

            case "BC Members":
                navigationHistory.Clear(); imgBack.IsVisible = false;
                NavigateTo(new BcMembersPage(), addToHistory: false);
                break;

            case "BC Sheet":
                navigationHistory.Clear(); imgBack.IsVisible = false;
                NavigateTo(new BcPage(startOnSummary: !Session.IsAdmin), addToHistory: false);
                break;
        }
    }
    
    private void ShowAdminPage()
    {
        var adminPage = new AdminPage();

        adminPage.OnEditRequested += (expenseId) =>
        {
            var editPage = new EditExpensePage(expenseId);
            editPage.OnSaved += () => OnBackClicked(null, null);
            NavigateTo(editPage);
        };

        NavigateTo(adminPage, addToHistory: false);
    }



}