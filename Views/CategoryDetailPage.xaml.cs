using DailyExpense.FirebaseClass;
using DailyExpense.HelperModel;
using DailyExpense.Models;
using Firebase.Database.Query;

namespace DailyExpense.Views;

public partial class CategoryDetailPage : ContentView
{
    private string categoryName;
    private DateTime currentMonth;
    private List<Expense> categoryExpenses = new List<Expense>();
    public event Action<string> OnEditRequested;

    public CategoryDetailPage(string categoryName, DateTime selectedMonth)
	{
		InitializeComponent();
        this.categoryName = categoryName;
        lblCategoryTitle.Text = categoryName;
       
        currentMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        _ = LoadFromFirebaseAsync();
    }

    public void Refresh()
    {
        _ = LoadFromFirebaseAsync();
    }

    private async Task LoadFromFirebaseAsync()
    {
        loader.Show();
        try
        {
            var result = await FirebaseService.Db
                .Child("expenses")
                .OrderBy("UserId")
                .EqualTo(Session.UserId)
                .OnceAsync<Expense>();

            categoryExpenses = result
                .Select(x =>
                {
                    x.Object.Id = x.Key;     // Firebase key = expense Id
                    return x.Object;
                })
                .Where(x => !x.IsDeleted && x.CategoryName == categoryName)
                .ToList();

            ShowItems();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Data load nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
        finally
        {
            loader.Hide();
        }
    }

    private void ShowItems()
    {
        lblSelectedMonth.Text = currentMonth.ToString("MMMM yyyy") + "  ▾";

        DateTime startDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        DateTime nextMonth = startDate.AddMonths(1);

        var items = categoryExpenses
            .Where(x => x.Date >= startDate && x.Date < nextMonth)
            .OrderByDescending(x => x.Date)
            .Select(x => new ExpenseItem
            {
                Id = x.Id,
                SamanName = x.SamanName,
                Date = x.Date,
                Amount = x.Amount,
                WeightDisplay = string.IsNullOrWhiteSpace(x.Unit)
                    ? x.SamanWeight
                    : $"{x.SamanWeight} {x.Unit}"
            })
            .ToList();

        cvItems.ItemsSource = items;
        lblNoData.IsVisible = items.Count == 0;
        cvItems.IsVisible = items.Count > 0;
    }

    private void OnPreviousMonth(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(-1);
        ShowItems();
    }

    private void OnNextMonth(object sender, EventArgs e)
    {
        var nextMonth = currentMonth.AddMonths(1);
        if (nextMonth <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
        {
            currentMonth = nextMonth;
            ShowItems();
        }
    }

    private void OnItemTapped(object sender, EventArgs e)
    {
        var item = (sender as Border)?.BindingContext as ExpenseItem;
        if (item == null) return;

        OnEditRequested?.Invoke(item.Id);
    }

    private async void OnMonthLabelTapped(object sender, EventArgs e)
    {
        var picked = await MonthPicker.PickAsync(currentMonth);
        if (picked == null) return;

        currentMonth = picked.Value;
        ShowItems();
    }
}