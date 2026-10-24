using DailyExpense.FirebaseClass;
using DailyExpense.Models;
using DailyExpense.Views.Admin.Model;
using Firebase.Database.Query;
using System.Globalization;
using BcMember = DailyExpense.Views.Admin.Model.BcMember;

namespace DailyExpense.Views.Admin;

public partial class BcPage : ContentView
{
    static readonly DateTime StartMonth = new DateTime(2026, 10, 1);   
    const int CycleMonths = 8;
    const int MaxMonths = 24;                                          
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private DateTime currentMonth;
    private bool showMonthTab = true;
    private bool isBusy = false;
    private List<BcMember> members = new List<BcMember>();
    private List<BcPayment> payments = new List<BcPayment>();
    
    public BcPage(bool startOnSummary)
	{
		InitializeComponent();
        var today = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        currentMonth = today < StartMonth ? StartMonth : today;

        SetTab(true);
        _ = LoadAsync();
    }

    public void Refresh() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        loader.Show();
        try
        {
            var m = await FirebaseService.Db.Child("bc").Child("members").OnceAsync<BcMember>();
            members = m.Select(x => { x.Object.Id = x.Key; return x.Object; })
                       .Where(x => x.IsActive)
                       .OrderBy(x => x.Order)
                       .ToList();

            var p = await FirebaseService.Db.Child("bc").Child("payments").OnceAsync<BcPayment>();
            payments = p.Select(x => { x.Object.Id = x.Key; return x.Object; }).ToList();

            await MainThread.InvokeOnMainThreadAsync(() => ShowData());
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

    private void ShowData()
    {
        string monthKey = currentMonth.ToString("yyyy-MM", Inv);
        lblMonth.Text = currentMonth.ToString("MMMM yyyy", Inv);
        int diff = (currentMonth.Year - StartMonth.Year) * 12 + currentMonth.Month - StartMonth.Month;
        int cycleIndex = diff / CycleMonths;
        DateTime cycleStart = StartMonth.AddMonths(cycleIndex * CycleMonths);
        DateTime cycleEnd = cycleStart.AddMonths(CycleMonths - 1);
        //lblCycle.Text = $"{cycleStart.ToString("MMM-yyyy", Inv)} to {cycleEnd.ToString("MMM-yyyy", Inv)}";
        lblCycle.Text = $"{cycleStart.ToString("MMM-yyyy", Inv)} to {currentMonth.ToString("MMM-yyyy", Inv)}  (cycle: {cycleStart.ToString("MMM-yyyy", Inv)} to {cycleEnd.ToString("MMM-yyyy", Inv)})";

        //string fromKey = cycleStart.ToString("yyyy-MM", Inv);
        //string toKey = cycleEnd.ToString("yyyy-MM", Inv);
        //string fromKey = cycleStart.ToString("yyyy-MM", Inv);
        //string toKey = monthKey;
        string fromKey = monthKey;
        string toKey = monthKey;

        var monthPayments = payments.Where(p => p.Month == monthKey).ToList();

        var rows = members.Select(m =>
        {
            decimal amt = monthPayments.FirstOrDefault(p => p.MemberId == m.Id)?.Amount ?? 0;
            return new BcRow
            {
                MemberId = m.Id,
                Name = m.Name,
                Amount = amt,
                AmountText = amt > 0 ? $"₹ {amt:N0}" : "Pending",
                AmountColor = amt > 0 ? "#2E7D32" : "#9E9E9E",
                RowColor = amt > 0 ? "#E8F5E9" : "#FFFFFF"
            };
        }).ToList();

        cvMonth.ItemsSource = rows;
        decimal monthTotal = rows.Sum(r => r.Amount);
        var cyclePayments = payments
            .Where(p => string.CompareOrdinal(p.Month, fromKey) >= 0 && string.CompareOrdinal(p.Month, toKey) <= 0)
            .ToList();

        decimal cycleTotal = cyclePayments.Sum(p => p.Amount);

        cvSummary.ItemsSource = members.Select(m =>
        {
            decimal t = cyclePayments.Where(p => p.MemberId == m.Id).Sum(p => p.Amount);
            return new BcSummaryRow
            {
                Name = m.Name,
                TotalText = $"₹ {t:N0}",
                PercentText = cycleTotal > 0 ? $"{Math.Round(t / cycleTotal * 100, 1)}%" : "0%"
            };
        }).ToList();
        if (showMonthTab)
        {
            lblTotalTitle.Text = currentMonth.ToString("MMM yyyy", Inv) + " Total";
            lblTotal.Text = $"₹ {monthTotal:N0}";
        }
        else
        {
            lblTotalTitle.Text = "Total till " + currentMonth.ToString("MMM yyyy", Inv);
            lblTotal.Text = $"₹ {cycleTotal:N0}";
        }
    }
    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        var row = (sender as Border)?.BindingContext as BcRow;
        if (row == null || isBusy) return;

        if (!Session.IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Message", "Only the admin can add entries.", "OK");
            return;
        }

        string monthKey = currentMonth.ToString("yyyy-MM", Inv);

        string input = await Application.Current.MainPage.DisplayPromptAsync(
            row.Name,
            $"{lblMonth.Text} Amount (Leave blank to remove the entry)",
            "Save", "Cancel",
            initialValue: row.Amount > 0 ? row.Amount.ToString("0.##", Inv) : "",
            keyboard: Keyboard.Numeric);

        if (input == null) return;         

        decimal amount = 0;
        if (!string.IsNullOrWhiteSpace(input) &&
            (!decimal.TryParse(input.Trim(), NumberStyles.Number, Inv, out amount) || amount < 0))
        {
            await Application.Current.MainPage.DisplayAlert("Message", "Please enter correct amount.", "OK");
            return;
        }

        isBusy = true;
        try
        {
            string key = $"{monthKey}_{row.MemberId}";
            var node = FirebaseService.Db.Child("bc").Child("payments").Child(key);

            if (amount > 0)
            {
                var pay = new BcPayment
                {
                    Id = key,
                    MemberId = row.MemberId,
                    Month = monthKey,
                    Amount = amount,
                    EnteredBy = Preferences.Get("FullName", "Admin"),
                    EnteredOn = DateTime.Now
                };
                await node.PutAsync(pay);

                payments.RemoveAll(p => p.Month == monthKey && p.MemberId == row.MemberId);
                payments.Add(pay);
            }
            else
            {
                await node.DeleteAsync();
                payments.RemoveAll(p => p.Month == monthKey && p.MemberId == row.MemberId);
            }

            ShowData();
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

    private void SetTab(bool month)
    {
        showMonthTab = month;
        cvMonth.IsVisible = month;
        cvSummary.IsVisible = !month;

        btnTabMonth.BackgroundColor = month ? Color.FromArgb("#0067EE") : Color.FromArgb("#E0E0E0");
        btnTabMonth.TextColor = month ? Colors.White : Colors.Black;
        btnTabSummary.BackgroundColor = month ? Color.FromArgb("#E0E0E0") : Color.FromArgb("#0067EE");
        btnTabSummary.TextColor = month ? Colors.Black : Colors.White;
    }

    private void OnTabMonth(object sender, EventArgs e) { SetTab(true); ShowData(); }
    private void OnTabSummary(object sender, EventArgs e) { SetTab(false); ShowData(); }
    private void OnPreviousMonth(object sender, EventArgs e)
    {
        if (currentMonth > StartMonth)
        {
            currentMonth = currentMonth.AddMonths(-1);
            ShowData();
        }
    }

    private void OnNextMonth(object sender, EventArgs e)
    {
        if (currentMonth < StartMonth.AddMonths(MaxMonths - 1))
        {
            currentMonth = currentMonth.AddMonths(1);
            ShowData();
        }
    }
}