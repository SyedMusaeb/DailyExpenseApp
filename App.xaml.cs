using DailyExpense.MasterView;
using DailyExpense.Models;
using DailyExpense.Views;

namespace DailyExpense
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
           
            bool isLoggedIn = Preferences.Get("IsLoggedIn", false);

            Page startPage = isLoggedIn ? new MasterPage() : new Login();

            return new Window(new Homepage());
        }
    }
}