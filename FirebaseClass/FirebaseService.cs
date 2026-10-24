using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.FirebaseClass
{

    public static class FirebaseService
    {
        const string ApiKey = "AIzaSyCRs2a-HOL5tWFrYAqPDM7JBrWU4TqaR8c";
        const string AuthDomain = "dailyexpense-a1d89.firebaseapp.com";
        const string DbUrl = "https://dailyexpense-a1d89-default-rtdb.asia-southeast1.firebasedatabase.app";

        static readonly FirebaseAuthClient auth = new FirebaseAuthClient(new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        });

        public static FirebaseClient Db => new FirebaseClient(DbUrl, new FirebaseOptions
        {
            AuthTokenAsyncFactory = () => auth.User.GetIdTokenAsync()
        });

        public static string Uid => auth.User?.Uid;

        public static string ToEmail(string phone) => $"{phone}@dailyexpense.app";

        public static async Task Register(string name, string phone, string password)
        {
            var cred = await auth.CreateUserWithEmailAndPasswordAsync(ToEmail(phone), password, name);

            await Db.Child("users").Child(cred.User.Uid).PutAsync(new
            {
                FullName = name,
                Phone = phone,
                UserType = "User",   // Admin sirf console se banega
                Status = "Active"
            });
        }

        public static async Task<bool> Login(string phone, string password)
        {
            try
            {
                await auth.SignInWithEmailAndPasswordAsync(ToEmail(phone), password);
                return true;
            }
            catch { return false; }
        }

        public static void Logout() => auth.SignOut();


        // old password verify karke naya password set karo (null = success)
        public static async Task<string> ChangePassword(string phone, string oldPassword, string newPassword)
        {
            try
            {
                await auth.SignInWithEmailAndPasswordAsync(ToEmail(phone), oldPassword);
            }
            catch
            {
                return "Mobile number or old password is wrong";
            }

            try
            {
                await auth.User.ChangePasswordAsync(newPassword);
                auth.SignOut();
                return null;
            }
            catch (Exception ex)
            {
                auth.SignOut();
                return "Error. " + ex.Message;
            }
        }
    }


}
