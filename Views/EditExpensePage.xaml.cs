using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using Firebase.Database.Query;

namespace DailyExpense.Views;

public partial class EditExpensePage : ContentView
{
    private Expense expense;
    private List<Category> categoryList;
    private bool isBusy = false;

    public event Action OnSaved;
    public EditExpensePage(string expenseId)
	{
		InitializeComponent();
        LoadData();

        _ = LoadAllAsync(expenseId);
    }

    private async Task LoadAllAsync(string expenseId)
    {
        try
        {
            // 1. categories load karo
            var catResult = await FirebaseService.Db
                .Child("Data")
                .Child("categories")
                .OnceAsync<Category>();

            categoryList = catResult
                .Select(x => x.Object)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Id)
                .ToList();

            // 2. expense load karo
            expense = await FirebaseService.Db
                .Child("expenses")
                .Child(expenseId)
                .OnceSingleAsync<Expense>();

            if (expense == null)
            {
                await Application.Current.MainPage.DisplayAlert("Message", "Expense not found.", "OK");
                OnSaved?.Invoke();
                return;
            }

            expense.Id = expenseId;

            // 3. ab Picker set karo aur screen bharo (yahi wo code hai)
            var names = categoryList.Select(c => c.CategoryName).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                pickerCategory.ItemsSource = names;
                LoadData();     // items set hone ke baad hi selection karo
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Data load nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
    }
    private void LoadData()
    {
        if (expense == null) return;

        //int index = categoryList.FindIndex(c => c.Id == expense.CategoryId);
        //if (index == -1)
            int index = categoryList.FindIndex(c => c.CategoryName == expense.CategoryName);

        pickerCategory.SelectedIndex = index;
        txtsamanname.Text = expense.SamanName;
        txtsamanweight.Text = expense.SamanWeight.ToString();
        //pickerUnit.SelectedItem = null;
        txtamount.Text = expense.Amount.ToString();
        //pickerPaymentMode.SelectedItem = expense.PaymentMode;
        //txtnotes.Text = expense.Notes;
        dpDate.Date = expense.Date;
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (isBusy || expense == null) return;
        isBusy = true;

        try
        {
            if (pickerCategory.SelectedIndex == -1)
            {
                await Application.Current.MainPage.DisplayAlert("Message", "Please select category.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtsamanname.Text))
            {
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter item name.", "OK");
                txtsamanname.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtsamanweight.Text))
            {
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter correct saman weight.", "OK");
                txtsamanweight.Focus();
                return;
            }

            if (!decimal.TryParse(txtamount.Text, out decimal amount) || amount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Message", "Please enter correct amount.", "OK");
                txtamount.Focus();
                return;
            }

            var cat = categoryList[pickerCategory.SelectedIndex];

            expense.CategoryId = cat.Id;
            expense.CategoryName = cat.CategoryName;
            expense.SamanName = txtsamanname.Text.Trim();
            expense.SamanWeight = txtsamanweight.Text.Trim();
            expense.Amount = amount;
            expense.Date = dpDate.Date;
            expense.ModifiedBy = Preferences.Get("FullName", "Unknown");
            expense.ModifiedOn = DateTime.Now;

            await FirebaseService.Db.Child("expenses").Child(expense.Id).PutAsync(expense);

            await Application.Current.MainPage.DisplayAlert("Success", "Expense Updated!", "OK");
            OnSaved?.Invoke();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Update nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }

    // ---- DELETE (soft delete) ----
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (isBusy || expense == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert("Delete", "Delete this expense?", "Yes", "No");
        if (!confirm) return;

        isBusy = true;
        try
        {
            expense.IsDeleted = true;
            expense.DeletedOn = DateTime.Now;
            expense.DeletedBy = Preferences.Get("FullName", "Unknown");

            await FirebaseService.Db.Child("expenses").Child(expense.Id).PutAsync(expense);

            OnSaved?.Invoke();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Delete nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }

    // ---- RESTORE (sirf admin) ----
    private async void OnRestoreClicked(object sender, EventArgs e)
    {
        if (isBusy || expense == null || !Session.IsAdmin) return;
        isBusy = true;

        try
        {
            expense.IsDeleted = false;
            expense.DeletedOn = null;
            expense.DeletedBy = null;
            expense.ModifiedBy = Preferences.Get("FullName", "Admin");
            expense.ModifiedOn = DateTime.Now;

            await FirebaseService.Db.Child("expenses").Child(expense.Id).PutAsync(expense);

            await Application.Current.MainPage.DisplayAlert("Success", "Expense Restored!", "OK");
            OnSaved?.Invoke();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                "Restore nahi hua. Internet check karo.\n" + ex.Message, "OK");
        }
        finally
        {
            isBusy = false;
        }
    }
}