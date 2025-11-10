using AstarLibrary.Models;
using AstarLibrary.Modules;
using AstarView.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AstarView
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;

        public NodeView[,] Nodes = new NodeView[10, 10];
        public AstarFinder pathFinder = new AstarFinder(10, 10);
        private DispatcherTimer timer = new DispatcherTimer();

        public bool ShowValues { get => showValues.IsChecked ?? false; }
        public bool IsDiagonal { get => isDiagonal.IsChecked ?? false; }

        public bool Started;

        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            Start();
        }
        private void Start()
        {
            GenMap();
            timer.Tick += new EventHandler(Update);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            ShowMap();
            if (Started)
            {
                pathFinder.SelectNextNode();
                if (pathFinder.PathFinished)
                {
                    Started = false;
                }

                isDiagonal.IsEnabled = false;
            }
            else
            {
                isDiagonal.IsEnabled = true;
            }
        }

        private void GenMap()
        {
            for (int y = 0; y < pathFinder.Height; y++)
            {
                for (int x = 0; x < pathFinder.Width; x++)
                {
                    Nodes[x, y] = new NodeView();
                    Nodes[x, y].Margin = new Thickness(2 + x * (Nodes[x, y].Width + 2), y * (Nodes[x, y].Height + 2), 0, 0);
                    Nodes[x, y].PosX = x;
                    Nodes[x, y].PosY = y;
                    gridView.Children.Add(Nodes[x, y]);
                }
            }
        }
        private void ShowMap()
        {
            foreach (var pathnode in pathFinder.NodesList.Cast<AstarNode>())
            {
                var nodeView = Nodes[pathnode.Pos.x, pathnode.Pos.y];
                if (ShowValues)
                {
                    nodeView.Gvalue.Text = pathnode.Gcost.ToString();
                    nodeView.Hvalue.Text = pathnode.Hcost.ToString();
                    nodeView.Fvalue.Text = pathnode.Fcost.ToString();
                    nodeView.Mvalue.Text = "x" + pathnode.Multiplier.ToString();
                }
                else
                {
                    nodeView.Gvalue.Text = "";
                    nodeView.Hvalue.Text = "";
                    nodeView.Fvalue.Text = "";
                    nodeView.Mvalue.Text = "";
                }

                if (pathnode.IsEndNode)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#00F"));
                else if (pathnode.IsStartNode)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#F00"));
                else if (pathnode.PathFound)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#0F0"));
                else if (pathnode.IsChecked)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#DDD"));
                else if (pathnode.Gcost > 0)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#AAA"));
                else if (pathnode.IsWall)
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#000"));
                else
                    nodeView.background.Background = (Brush)(new BrushConverter().ConvertFrom("#EEE"));
            }
        }

        public void AddMultiplier(int posX, int posY)
        {
            if (Started || !float.TryParse(multiplier.Text.Replace(".", ","), out float mult)) return;

            pathFinder.Reset();
            var node = pathFinder.GetNode(posX, posY);
            node.SetCostMultiplier(mult);
        }
        private void StartPath_Click(object sender, RoutedEventArgs e)
        {
            pathFinder.Reset();
            pathFinder.IsDiagonal = IsDiagonal;
            Started = true;
        }
        private void WallMultiplier_Click(object sender, RoutedEventArgs e)
        {
            multiplier.Text = "0";
        }
        private void PathMultiplier_Click(object sender, RoutedEventArgs e)
        {
            multiplier.Text = "1";
        }
    }
}
