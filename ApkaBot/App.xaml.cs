using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ApkaBotLibrary;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;



namespace ApkaBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppCenter.Start("d9808f92-31ae-46e0-811d-9ed8784d2474", typeof(Analytics), typeof(Crashes));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Apka.GetInstance.Login == null)
            {
                return;
            }

            Analytics.TrackEvent("APP EXITED", new Dictionary<string, string> {
                { Apka.GetInstance.Login, $"{Apka.GetInstance.Req.GetCookies.FirstOrDefault(x => x.Name == "user_id").Value}" }
            });
        }
    }
}
