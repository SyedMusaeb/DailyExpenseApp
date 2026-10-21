using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using Firebase.Database.Query;
using System.Text.RegularExpressions;
using System.Xml.Linq;
namespace DailyExpense.Views;

public partial class DailyExpenseForm : ContentView
{
    private bool isDateSelected = false;
    private List<Category> categoryList;
    public event Action GoToSamanList;
    private bool isBusy = false;
    public DailyExpenseForm()
	{
		InitializeComponent();
        dpDate.MaximumDate = DateTime.Today;
        dpDate.Date = DateTime.Now;
        //LoadCategories();
        _ = LoadCategoriesAsync();

    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await FirebaseService.Db
                .Child("Data")
                .Child("categories")
                .OnceAsync<Category>();
            categoryList = result
                .Select(x => x.Object)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Id)
                .ToList();

            pickerCategory.ItemsSource = categoryList.Select(c => c.CategoryName).ToList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void expense_Clicked(object sender, EventArgs e)
    {
        if (isBusy) return;
        isBusy = true;

        try
        {
            ResetBorders();

            // ---- CATEGORY ----
            if (pickerCategory.SelectedIndex == -1)
            {
                borderCategory.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please select category.", "OK");
                pickerCategory.Focus();
                return;
            }

            // ---- ITEM NAME ----
            if (string.IsNullOrWhiteSpace(txtsamanname.Text))
            {
                borderSamanName.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter item name.", "OK");
                txtsamanname.Focus();
                return;
            }

            // ---- WEIGHT ----
            if (string.IsNullOrWhiteSpace(txtsamanweight.Text))
            {
                borderWeight.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter correct saman weight.", "OK");
                txtsamanweight.Focus();
                return;
            }

            // ---- AMOUNT ----
            if (!decimal.TryParse(txtamount.Text, out decimal amountValue) || amountValue <= 0)
            {
                borderAmount.Stroke = Colors.Red;
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter correct amount.", "OK");
                txtamount.Focus();
                return;
            }

            Category selectedCategory = categoryList[pickerCategory.SelectedIndex];

            var expense = new Expense
            {
                UserId = Session.UserId,
                CategoryId = selectedCategory.Id,
                CategoryName = selectedCategory.CategoryName,
                SamanName = txtsamanname.Text.Trim(),
                SamanWeight = txtsamanweight.Text.Trim(),
                Amount = amountValue,
                Date = dpDate.Date,
                IsDeleted = false,
                CreatedBy = Preferences.Get("FullName", "Unknown"),
                CreatedOn = DateTime.Now
            };

            await FirebaseService.Db.Child("expenses").PostAsync(expense);

            await Application.Current.MainPage.DisplayAlert("Success", "Expense Saved!", "OK");
            ClearForm();
            ResetBorders();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Expense save nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }
    private void ClearForm()
    {
        pickerCategory.SelectedIndex = -1;
        txtsamanname.Text = string.Empty;
        txtsamanweight.Text = string.Empty;
        txtamount.Text = string.Empty;
        dpDate.Date = DateTime.Now;
    }

    private void OnViewListClicked(object sender, EventArgs e)
    {
        GoToSamanList?.Invoke();
    }
    private void ResetBorders()
    {
        borderCategory.Stroke = Colors.Gray;
        borderSamanName.Stroke = Colors.Gray;
        borderWeight.Stroke = Colors.Gray;
        borderAmount.Stroke = Colors.Gray;
    }
}
