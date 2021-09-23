using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApkaBotLibrary.Utils.Settings
{
    public class FileConnector : ISettingsConnection
    {
        private string url = $"settings/{Apka.GetInstance.Login}_settings.json";

        public dynamic Settings { get; set; } = new
        {
            Parallel = true,
            Characters = new Dictionary<string, SettingsModel>()
        };

        public void GetSettings()
        {
            //If file not found
            if (!File.Exists(url))
            {
                Settings.Characters.Add("ID_Postaci", new SettingsModel());

                using StreamWriter outputFile = new StreamWriter(url);

                outputFile.WriteLine($"//Link do poradnika: https://pastebin.com/enFM3iWY");
                outputFile.Write(JsonConvert.SerializeObject(Settings, Formatting.Indented));

                return;
            }

            using StreamReader r = new StreamReader(url);

            string json = r.ReadToEnd();

            try
            {
                Settings = JsonConvert.DeserializeAnonymousType(json, Settings);
            }
            catch (Exception)
            {
                //Meant to be empty
            }
        }

        public void SetSettings()
        {
            if (Settings == null)
            {
                return;
            }

            Apka.GetInstance.ParallelAttacking = Settings.Parallel;

            foreach (var player in Apka.GetInstance.Characters)
            {
                Settings.Characters.TryGetValue(player.Key, out SettingsModel settingsModel);
                if (settingsModel == null)
                {
                    continue;
                }

                player.Value.SetSettings(settingsModel);
            }
        }

        public bool SaveSettings(Dictionary<string, SettingsModel> settings)
        {
            throw new NotImplementedException();
        }
    }
}
