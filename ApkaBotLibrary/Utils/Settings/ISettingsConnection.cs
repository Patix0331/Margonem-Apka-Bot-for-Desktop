using ApkaBotLibrary.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Settings
{
    public interface ISettingsConnection
    {
        dynamic Settings { get; set; }

        void GetSettings();

        bool SaveSettings(Dictionary<string, SettingsModel> settings);

        void SetSettings();
    }
}
