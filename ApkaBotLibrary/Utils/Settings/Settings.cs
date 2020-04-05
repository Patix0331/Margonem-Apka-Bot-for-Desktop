using ApkaBotLibrary.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Settings
{
    public class Settings
    {
        #region HEALING
        /// <summary>
        /// Do you have to heal after battle
        /// </summary>
        public bool AutoHeal = true;
        /// <summary>
        /// Do you have to use fullheal potions
        /// </summary>
        public bool FullHeal = false;
        /// <summary>
        /// Heal yourself if your health is lower than set percentage
        /// </summary>
        public int AutoHealHP = 80;
        #endregion
        #region SHOP
        public bool AutoSell = true;
        #endregion
        #region AUTO EQUIP
        public bool EquipBetterItems { get; set; }
        #endregion
        #region STAMINA
        public bool UseStamina { get; set; }
        #endregion
        #region MAP SETTINGS
        public MobileMaps Map { get; set; }
        #endregion
    }
}
