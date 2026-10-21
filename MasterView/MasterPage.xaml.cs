using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using DailyExpense.Views;
using DailyExpense.Views.Admin;

namespace DailyExpense.MasterView;

public partial class MasterPage : ContentPage
{
    private Stack<View> navigationHistory = new Stack<View>();
    public MasterPage()
	{
		InitializeComponent();
        LoadUserSession();
        //PageContent.Content = new DailyExpenseForm();
        //ShowExpenseForm();

        lblMenu.IsVisible = Session.IsAdmin;

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
        string choice = await DisplayActionSheet("Menu", "Cancel", null,
            "Add Expense", "Expense List", "Admin Panel");

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