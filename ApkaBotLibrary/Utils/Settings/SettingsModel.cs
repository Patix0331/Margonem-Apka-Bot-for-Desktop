using ApkaBotLibrary.Requests;
using ApkaBotLibrary.Requests.Client.Shop;
using ApkaBotLibrary.Requests.SendGold;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Settings
{
    public class SettingsModel
    {
        #region MANAGING EARNED GOLD
        public SendGold SendGold { get; set; } = new SendGold();
        #endregion
        #region HEALING
        /// <summary>
        /// Do you have to use fullheal potions
        /// </summary>
        public bool FullHeal { get; set; }
        /// <summary>
        /// Heal yourself if your health is lower than set percentage
        /// </summary>
        public int AutoHealHP = 80;
        public int MinValueOfHeal { get; set; } = 500;
        #endregion
        #region SHOP
        //public bool AutoSell = true;
        public bool SellingPotions { get; set; }
        public Shop ClientShop { get; set; } = new Shop();
        #endregion
        #region AUTO EQUIP
        //public bool EquipBetterItems { get; set; }
        public bool UseBless { get; set; }

        public string BlessName { get; set; }
        #endregion
        #region STAMINA
        public bool UseStamina { get; set; }
        #endregion
        #region MAP SETTINGS
        public MobileMaps Map { get; set; } = new MobileMaps();
        #endregion
    }

}