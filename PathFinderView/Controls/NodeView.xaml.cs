using AstarLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstarView.Controls
{
    /// <summary>
    /// Logique d'interaction pour NodeView.xaml
    /// </summary>
    public partial class NodeView : UserControl
    {
        public int PosX { get; set; }
        public int PosY { get; set; }

        public NodeView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.instance.AddMultiplier(PosX, PosY);
        }
    }
}
