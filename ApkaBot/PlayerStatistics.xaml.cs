using ApkaBotLibrary;
using ApkaBotLibrary.Account;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ApkaBot_WPF
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private List<Character> Characters { get; set; }

        public Page1()
        {
            foreach (var item in Apka.GetInstance.Characters)
            {
                Characters.Add(item.Value);
            }

            InitializeComponent();
        }
    }
}
