using ApkaBotLibrary;
using Microsoft.AppCenter;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ApkaBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            using (var update = new WebClient())
            {
                string v = "1.0.9.2";
                string updateUrl = "https://1drv.ms/u/s!AnyDyWK-pS6MgaIwxWz3hvB0i7VjjQ?e=bsAmmF";
                string a = update.DownloadString(new Uri("https://pastebin.com/raw/1puJUje2"));
                if (!a.Contains(v))
                {
                    if (a.Contains("force_update"))
                    {
                        MessageBox.Show("Masz starą wersję bota. Musisz pobrać nową!", $"ApkaBot v{v} - ForceUpdate", MessageBoxButton.OK, MessageBoxImage.Information);
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {updateUrl}"));
                        Environment.Exit(1);
                    }

                    if (MessageBox.Show("Masz starą wersję bota. Czy chcesz pobrać nową?", $"ApkaBot v{v} - Update", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {updateUrl}"));
                    }
                }
            }
        }
        /// <summary>
        /// Login to game and change to characters view
        /// </summary>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (login.Text.Length <= 3 || password.Password.Length <= 3 || (bool)isProxy.IsChecked && Proxy.Text.Length <= 9)
            {
                return;
            }
            WebProxy proxy = null;
            if ((bool)isProxy.IsChecked)
            {
                proxy = new WebProxy
                {
                    Address = new Uri($"http://{Proxy.Text}")
                };
                proxy.Credentials = new NetworkCredential(userName: ProxyLogin.Text, password: ProxyPassword.Password);
            }

            signIn.IsEnabled = false;
            int logged = await Apka.GetInstance.Req.Login(login.Text, password.Password, proxy);

            if (logged == 0)
            {
                MessageBox.Show("Błędne hasło lub login!", "ApkaBot - Error");
                await Task.Delay(500);
                signIn.IsEnabled = true;
                return;
            }
            else if (logged == -1)
            {
                Apka.GetInstance.ResetReq();
                MessageBox.Show("Błędne proxy!", "ApkaBot - Error");
                signIn.IsEnabled = true;
                return;
            }

            AppCenter.SetUserId(login.Text);
            CharactersForm charactersForm = new CharactersForm();
            charactersForm.Show();
            Apka.GetInstance.Login = login.Text;
            this.Close();
        }

        private void isProxy_Changed(object sender, RoutedEventArgs e)
        {
            var isChecked = (bool)isProxy.IsChecked;
            Proxy.IsEnabled = isChecked;
            ProxyLogin.IsEnabled = isChecked;
            ProxyPassword.IsEnabled = isChecked;
        }
    }
}
