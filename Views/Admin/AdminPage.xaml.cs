using DailyExpense.FirebaseClass;
using DailyExpense.HelperModel;
using DailyExpense.Models;
using DailyExpense.Views.Admin.Model;

namespace DailyExpense.Views.Admin;

public partial class AdminPage : ContentView
{
    public event Action<string> OnEditRequested;

    private DateTime currentMonth;
    //private List<NewRegistration> userList;
    private List<NewRegistration> userList = new List<NewRegistration>();
    private List<Expense> allExpenses = new List<Expense>();  
    private bool isReady = false;
    public AdminPage()
    {
        InitializeComponent();

        currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        //userList = App.db.Table<NewRegistration>().ToList();

        var names = new List<string> { "All Users" };
        names.AddRange(userList.Select(u => u.FullName));
        pickerUser.ItemsSource = names;
        pickerUser.SelectedIndex = 0;
        _ = LoadUsersAndExpensesAsync();

    }

    public void Refresh() => _ = LoadUsersAndExpensesAsync();

    private async Task LoadUsersAndExpensesAsync()
    {
        loader.Show();
        try
        {
            var userResult = await FirebaseService.Db
                .Child("users")
                .OnceAsync<NewRegistration>();

            userList = userResult
                .Select(x =>
                {
                    x.Object.UserI = x.Key;       // Firebase Uid
                    return x.Object;
                })
                .OrderBy(u => u.FullName)
                .ToList();

            await FetchExpensesAsync();

            var names = new List<string> { "All Users" };
            names.AddRange(userList.Select(u => u.FullName));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                pickerUser.ItemsSource = names;
                pickerUser.SelectedIndex = 0;
                isReady = true;
                ShowData();
            });
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

    private async Task FetchExpensesAsync()
    {
        var result = await FirebaseService.Db
            .Child("expenses")
            .OnceAsync<Expense>();

        allExpenses = result
            .Select(x =>
            {
                x.Object.Id = x.Key;           // Firebase key = expense Id
                return x.Object;
            })
            .ToList();
    }
    private void ShowData()
    {
        if (!isReady) return;

        lblSelectedMonth.Text = currentMonth.ToString("MMMM yyyy") + "  ▾";
        lblMonthLabel.Text = currentMonth.ToString("MMM yyyy") + " Total";

        DateTime startDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        DateTime nextMonth = startDate.AddMonths(1);

        var expenses = allExpenses
            .Where(x => x.Date >= startDate && x.Date < nextMonth)
            .ToList();

        // user filter (index 0 = All Users)
        if (pickerUser.SelectedIndex > 0)
        {
            string userId = userList[pickerUser.SelectedIndex - 1].UserI;
            expenses = expenses.Where(x => x.UserId == userId).ToList();
        }

        lblTotal.Text = $" {expenses.Where(x => !x.IsDeleted).Sum(x => x.Amount):N0}";
        lblDeletedCount.Text = expenses.Count(x => x.IsDeleted).ToString();

        if (!swDeleted.IsToggled)
            expenses = expenses.Where(x => !x.IsDeleted).ToList();

        var items = expenses
            .OrderByDescending(x => x.Date)
            .Select(x => new AdminExpenseItem
            {
                Id = x.Id,
                SamanName = x.SamanName,
                CategoryName = x.CategoryName,
                UserName = userList.FirstOrDefault(u => u.UserI == x.UserId)?.FullName ?? "Unknown",
                Date = x.Date,
                Amount = x.Amount,
                WeightDisplay = string.IsNullOrWhiteSpace(x.Unit)
                    ? x.SamanWeight
                    : $"{x.SamanWeight} {x.Unit}",
                IsDeleted = x.IsDeleted,
                StatusText = x.IsDeleted ? "Deleted" : "",
                CardColor = x.IsDeleted ? "#FDECEC" : "#FFFFFF"
            })
            .ToList();

        cvAdmin.ItemsSource = items;
    }

    private void OnFilterChanged(object sender, EventArgs e) => ShowData();
    private void OnSwitchToggled(object sender, ToggledEventArgs e) => ShowData();

    private void OnPreviousMonth(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(-1);
        ShowData();
    }

    private void OnNextMonth(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(1);
        ShowData();
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        var item = (sender as Border)?.BindingContext as AdminExpenseItem;
        if (item != null)
            OnEditRequested?.Invoke(item.Id);
    }

    private async void OnMonthLabelTapped(object sender, EventArgs e)
    {
        var picked = await MonthPicker.PickAsync(currentMonth);
        if (picked == null) return;

        currentMonth = picked.Value;
        ShowData();
    }
}