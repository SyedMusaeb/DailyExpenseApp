using DailyExpense.FirebaseClass;
using DailyExpense.HelperModel;
using DailyExpense.Models;
using Firebase.Database.Query;
using System.Collections.ObjectModel;

namespace DailyExpense.Views;

public partial class SamanListPage : ContentView
{
    public event Action<string, DateTime> OnCategorySelected;
    private DateTime currentMonth;
    private List<Expense> userExpenses = new List<Expense>();
    public SamanListPage()
	{
		InitializeComponent();
        //LoadCategorySummary();
        currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //LoadTotalAmount();
       // LoadData();
        _ = LoadFromFirebaseAsync();
    }
    public void Refresh()
    {
        //LoadData();
        _ = LoadFromFirebaseAsync();
    }

    private async Task LoadFromFirebaseAsync()
    {
        try
        {
            var result = await FirebaseService.Db
                .Child("expenses")
                .OrderBy("UserId")
                .EqualTo(Session.UserId)
                .OnceAsync<Expense>();

            userExpenses = result
                .Select(x =>
                {
                    x.Object.Id = x.Key;
                    return x.Object;
                })
                .Where(x => !x.IsDeleted)
                .ToList();

            ShowData();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Data load nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
    }

    private void ShowData()
    {
        // all time total
        lblTotalAmount.Text = $"₹ {userExpenses.Sum(x => x.Amount):N0}";

        // selected month
        lblSelectedMonth.Text = currentMonth.ToString("MMMM yyyy");
        lblMonthLabel.Text = currentMonth.ToString("MMMM yyyy") + " Total";

        DateTime startDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        DateTime nextMonth = startDate.AddMonths(1);

        var monthExpenses = userExpenses
            .Where(x => x.Date >= startDate && x.Date < nextMonth)
            .ToList();

        lblMonthTotal.Text = $"₹ {monthExpenses.Sum(x => x.Amount):N0}";

        // category-wise list
        var summaryList = monthExpenses
            .GroupBy(x => x.CategoryName)
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                CategoryIcon = GetCategoryIcon(g.Key),
                MonthYear = currentMonth.ToString("MMMM yyyy"),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.CategoryName)
            .ToList();

        cvSamanList.ItemsSource = new ObservableCollection<CategorySummary>(summaryList);
    }

    private string GetCategoryIcon(string category)
    {
        switch (category)
        {
            case "Grocery": return "grocery.png";
            case "Sabzi": return "vegetable.png";
            case "Doodh": return "milk.png";
            default: return "other.png";
        }
    }

    private void OnCategoryTapped(object sender, EventArgs e)
    {
        var border = sender as Border;
        var selectedCategory = border?.BindingContext as CategorySummary;

        if (selectedCategory != null)
        {
            OnCategorySelected?.Invoke(selectedCategory.CategoryName, currentMonth);
        }
    }

    private void OnPreviousMonth(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(-1);
        ShowData();      // internet ki zaroorat nahi
    }

    private void OnNextMonth(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(1);
        ShowData();
    }
}