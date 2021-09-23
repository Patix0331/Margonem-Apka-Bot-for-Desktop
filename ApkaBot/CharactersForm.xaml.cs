using ApkaBotLibrary;
using ApkaBotLibrary.Account;
using ApkaBotLibrary.Requests;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ApkaBot
{
    /// <summary>
    /// Interaction logic for CharactersForm.xaml
    /// </summary>
    public partial class CharactersForm : Window
    {
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private bool isExited { get; set; }

        private List<Character> AvailableCharactersList;

        private DispatcherTimer Timer = new DispatcherTimer();
        private Stopwatch Stopwatch = new Stopwatch();

        private DispatcherTimer StaminaTimer = new DispatcherTimer();

        public CharactersForm()
        {
            InitializeComponent();

            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            Timer.Start();
        }

        /// <summary>
        /// Reload Lists on window
        /// </summary>
        private void WireUpLists()
        {
            AvailableCharacters.ItemsSource = AvailableCharactersList.OrderBy(x => x.World);
            //AvailableCharacters.DisplayMemberPath = "FullName";

            ListOfMaps.ItemsSource = AvailableCharactersList.First().Maps;
            ListOfMaps.DisplayMemberPath = "Name";
        }

        /// <summary>
        /// Function which will save/read settings from character based on given parametr
        /// </summary>
        /// <param name="character">Player</param>
        /// <param name="isSetter">true: saving, false: reading</param>
        private void Settings(Character character, bool isSetter)
        {
            if (character is null)
            {
                return;
            }

            if (isSetter)
            {
                character.GetSettings.Map = (MobileMaps)ListOfMaps.SelectedValue;
                character.GetSettings.AutoHealHP = (int)HealingPercentageSlider.Value;
                character.GetSettings.FullHeal = (bool)FullHealCheckbox.IsChecked;

                character.GetSettings.UseStamina = (bool)UseStamina.IsChecked;

                if (int.TryParse(minPotionHeal.Text, out int minHeal))
                {
                    character.GetSettings.MinValueOfHeal = minHeal;
                }
            }
            else
            {
                var playerMap = AvailableCharactersList.First().Maps.FirstOrDefault(x => x.Id == character.GetSettings.Map.Id);
                if (playerMap == null)
                {
                    character.GetSettings.Map = new MobileMaps();
                }

                ListOfMaps.SelectedValue = AvailableCharactersList.First().Maps.First(x => x.Id == character.GetSettings.Map.Id);
                HealingPercentageSlider.Value = character.GetSettings.AutoHealHP;
                FullHealCheckbox.IsChecked = character.GetSettings.FullHeal;

                UseStamina.IsChecked = character.GetSettings.UseStamina;

                minPotionHeal.Text = $"{character.GetSettings.MinValueOfHeal}";
            }
        }

        /// <summary>
        /// Send requests which received characters then fill ComboBox with returned characters
        /// Also makes init to load list of maps
        /// </summary>
        private async void LoadCharacters(object sender, RoutedEventArgs e)
        {
            bool state = await Apka.GetInstance.Req.Characters();

            if (!state)
            {
                MessageBox.Show("Wystąpił błąd podczas pobierania postaci!", "ApkaBot - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception("Wystąpił błąd podczas pobierania postaci!");
            }

            AvailableCharactersList = Apka.GetInstance.Characters.Values.ToList<Character>();

            await Apka.GetInstance.Req.Init((Character)AvailableCharactersList.First());

            var config = new ApkaBotLibrary.Utils.Settings.GlobalConfig();

            config.Connection.GetSettings();
            config.Connection.SetSettings();

            WireUpLists();
            Settings((Character)AvailableCharacters.SelectedItem, false);

            StaminaTimer.Tick += new EventHandler(AddStamina);
            StaminaTimer.Interval = new TimeSpan(0, 1, 0);
            StaminaTimer.Start();
        }

        private void AddStamina(object sender, EventArgs e)
        {
            foreach (var item in Apka.GetInstance.Characters)
            {
                if (item.Value.h.stamina == 50)
                {
                    continue;
                }

                item.Value.h.stamina++;
            }
        }

        /// <summary>
        /// Shows MessageBox with cookies from account
        /// </summary>
        private void ShowCookies_Click(object sender, RoutedEventArgs e)
        {
            var cookies = Apka.GetInstance.Req.GetCookies.ToList();
            if (MessageBox.Show($"{cookies[0]}\n{cookies[1]}\n{cookies[2]}\nPress OK to copy.", "ApkaBot - Cookies", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string c = "";
                foreach (var cookie in cookies)
                {
                    c += $"document.cookie=`{cookie.Name}={cookie.Value};domain=.margonem.pl`;";
                }
                Clipboard.SetText(c);
            }
        }

        /// <summary>
        /// Button which will invoke save settings function
        /// </summary>
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings((Character)AvailableCharacters.SelectedItem, true);
        }

        /// <summary>
        /// Slider managing percent from which value bot will start healing character
        /// </summary>
        /// <param name="e">Slider object itself</param>
        private void HealingPercentageSliderHealingPercentageSlider_OnChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HealingPercentage is null)
            {
                return;
            }

            if (e.NewValue < 0.1F)
            {
                HealingPercentage.Content = "Nie lecz w ogóle.";
            }
            else
            {
                HealingPercentage.Content = $"Leczenie od {e.NewValue / 100:P1}";
            }
        }

        /// <summary>
        /// Stoping bot and reset CancellationTokenSource
        /// </summary>
        private async void StopBotButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("Bot stopped", new Dictionary<string, string> {
                { Apka.GetInstance.Login, Apka.GetInstance.Req.GetCookies.FirstOrDefault(x => x.Name == "user_id").Value}
            });

            Apka.GetInstance.Exit = true;

            foreach (var item in Apka.GetInstance.Characters)
            {
                item.Value.Pause = true;
            }

            PauseCharacterButton.Content = "Start";

            cancellationToken.Cancel();
            cancellationToken.Dispose();
            cancellationToken = new CancellationTokenSource();

            Stopwatch.Stop();

            botStatus.Content = "Status: Zatrzymany";

            StopBotButton.IsEnabled = false;

            while (!isExited)
            {
                await Task.Delay(200);
            }

            isExited = false;

            StartBotButton.IsEnabled = true;
        }

        /// <summary>
        /// Start bot on click
        /// </summary>
        private async void StartBotButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("Bot started", new Dictionary<string, string> {
                { Apka.GetInstance.Login, Apka.GetInstance.Req.GetCookies.FirstOrDefault(x => x.Name == "user_id").Value}
            });

            Apka.GetInstance.Exit = false;

            StartBotButton.IsEnabled = false;
            StopBotButton.IsEnabled = true;

            Stopwatch.Restart();

            StartBot(cancellationToken.Token);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RefreshSatistics();
        }

        public void RefreshSatistics()
        {
            WorkTime.Content = $"Boci od: {Stopwatch.Elapsed.Hours}h {Stopwatch.Elapsed.Minutes}m {Stopwatch.Elapsed.Seconds}s";

            Character p = (Character)AvailableCharacters.SelectedItem;
            if (p == null)
            {
                return;
            }

            if (!Apka.GetInstance.ParallelAttacking)
            {
                OnWhichCharacterAtm.Visibility = Visibility.Visible;
                OnWhichCharacterAtm.Content = $"Aktualnie expi: {Apka.GetInstance.OnWhichCharacterAtm}";
            }

            nick.Content = $"{p.h.nick}({p.h.lvl}{p.h.prof})";
            playerStatus.Content = $"Status: {p.StatusInfo}";
            freeSlots.Content = $"Wolne sloty: {p.FreeSpaceLeft}";

            gold.Content = $"{p.h.gold:N0}/{p.h.goldlim:N0}$";
            gainGold.Content = $"Zdobyto: {p.h.gold - p.StartGold:N0}$";
            goldPerStamina.Content = $"~ gold na 1 staminę: {(p.h.gold + 1 - p.StartGold) / ((ulong)p.StaminaCounter + 1):N0}$";

            hp.Content = $"HP: {p.h.warrior_stats.hp:N0}/{p.h.warrior_stats.maxhp:N0} ({100 * p.h.warrior_stats.hp / p.h.warrior_stats.maxhp:F1}%)";
            healthBar.Maximum = p.h.warrior_stats.maxhp;
            healthBar.Value = p.h.warrior_stats.hp;
            healthBar.Visibility = Visibility.Visible;
            healPowerLeft.Content = $"Lek z potek: {p.HealPowerLeft}";

            expBar.Maximum = Math.Pow(p.h.lvl, 4) - Math.Pow(p.h.lvl - 1, 4);
            expBar.Value = p.h.exp - (10 + Math.Pow(p.h.lvl - 1, 4));
            expBar.Visibility = Visibility.Visible;
            gainExp.Content = $"Zdobyto: {p.h.exp - p.StartExp:N0} expa";
            exp.Content = $"EXP: {p.h.exp:N0}/{Math.Pow(p.h.lvl, 4):N0} ({100 * (p.h.exp - Math.Pow(p.h.lvl - 1, 4)) / (Math.Pow(p.h.lvl, 4) - Math.Pow(p.h.lvl - 1, 4)):F1}%)";

            wyczerp.Content = $"Wyczerpanie: {p.h.ttl}";
            stamina.Content = $"Stamina: {p.h.stamina}/50";

            if (p.h.prof == 't' || p.h.prof == 'h')
            {
                ammoLeft.Visibility = Visibility.Visible;
                ammoLeft.Content = $">>-|> {p.ArrowsLeft}";
            }

            deathCounter.Content = $"Padłeś: {p.DeathCounter} razy";
        }

        /// <summary>
        /// Infinity loop managing function of characters grouping in bot (grouped by world)
        /// </summary>
        private async void StartBot(CancellationToken cancellationToken)
        {
            while (true)
            {
                //TODO cancellationToken or any way to stop bot.
                //List<Task<bool>> tasks = new List<Task<bool>>();
                botStatus.Content = "Status: Zbija";

                List<Task> tasks = new List<Task>();

                StaminaTimer.Interval = new TimeSpan(0, 1, 0);


                if (Apka.GetInstance.ParallelAttacking)
                {
                    foreach (var world in Apka.GetInstance.NumberPerWorld)
                    {
                        var players = Apka.GetInstance.Characters.Where(x => x.Value.World == world.Key).Select(x => x.Value).ToList();
                        tasks.Add(Apka.GetInstance.Req.MainMethod(players));
                    }

                    await Task.WhenAll(tasks);
                }
                else
                {
                    foreach (var world in Apka.GetInstance.NumberPerWorld)
                    {
                        var players = Apka.GetInstance.Characters.Where(x => x.Value.World == world.Key).Select(x => x.Value).ToList();
                        await Apka.GetInstance.Req.MainMethod(players);

                        if (Apka.GetInstance.Exit)
                        {
                            OnWhichCharacterAtm.Content = "";
                            botStatus.Content = "Status: Wyłączony";
                            isExited = true;
                            return;
                        }
                    }

                    OnWhichCharacterAtm.Content = "";
                }

                if (Apka.GetInstance.Exit)
                {
                    botStatus.Content = "Status: Wyłączony";
                    isExited = true;
                    return;
                }

                botStatus.Content = "Status: Przerwa";

                while (true)
                {
                    bool isPaused = false;
                    foreach (var item in Apka.GetInstance.Characters)
                    {
                        if (item.Value.h.stamina >= 50 && !item.Value.Pause)
                        {
                            isPaused = true;
                            break;
                        }
                    }

                    if (isPaused)
                    {
                        break;
                    }
                    //Check is stop button pressed.
                    if (!StopBotButton.IsEnabled)
                    {
                        isExited = true;
                        return;
                    }

                    await Task.Delay(5000);
                }
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Change player pause state on click.
        /// </summary>
        private void PauseCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            Character player = (Character)AvailableCharacters.SelectedItem;
            if (player == null)
            {
                return;
            }

            if (player.Pause)
            {
                player.Pause = false;
                PauseCharacterButton.Content = "Stop";
            }
            else
            {
                player.Pause = true;
                PauseCharacterButton.Content = "Start";
            }
            Settings(player, true);
        }

        /// <summary>
        /// Change pause state on click
        /// </summary>
        private void WorldPause_Click(object sender, RoutedEventArgs e)
        {
            Character player = (Character)AvailableCharacters.SelectedItem;
            if (player == null)
            {
                return;
            }

            var worldPause = Apka.GetInstance.NumberPerWorld[player.World];

            if (worldPause)
            {
                Apka.GetInstance.NumberPerWorld[player.World] = false;
                foreach (var item in Apka.GetInstance.Characters)
                {
                    if (item.Value.World == player.World)
                    {
                        item.Value.Pause = false;
                        PauseCharacterButton.Content = "Stop";
                    }
                }
                player.Pause = false;
                WorldPause.Content = "Stop (Cały świat)";
            }
            else
            {
                Apka.GetInstance.NumberPerWorld[player.World] = true;
                foreach (var item in Apka.GetInstance.Characters)
                {
                    if (item.Value.World == player.World)
                    {
                        item.Value.Pause = true;
                        PauseCharacterButton.Content = "Start";
                    }
                }
                worldPause = true;
                player.Pause = true;
                WorldPause.Content = "Start (Cały świat)";
            }
        }

        /// <summary>
        /// Get player pause state and change button content text on selection changed.
        /// </summary>
        private void AvailableCharacters_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshSatistics();

            Character player = (Character)AvailableCharacters.SelectedItem;

            if (player == null)
            {
                return;
            }

            Settings(player, false);

            if (Apka.GetInstance.NumberPerWorld[player.World])
            {
                WorldPause.Content = "Wznów (świat)";
            }
            else
            {
                WorldPause.Content = "Pauza (świat)";
            }

            if (player.Pause)
            {
                PauseCharacterButton.Content = "Start";
            }
            else
            {
                PauseCharacterButton.Content = "Stop";
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (AvailableCharacters.SelectedIndex == 0)
                {
                    return;
                }
                else
                {
                    AvailableCharacters.SelectedIndex--;
                }
            }

            if (e.Key == Key.Down)
            {
                if (AvailableCharacters.SelectedIndex == AvailableCharacters.Items.Count - 1)
                {
                    return;
                }
                else
                {
                    AvailableCharacters.SelectedIndex++;
                }
            }

            if (e.Key == Key.S && e.Key == Key.LeftCtrl)
            {
                Settings((Character)AvailableCharacters.SelectedItem, true);
            }
        }
    }
}
