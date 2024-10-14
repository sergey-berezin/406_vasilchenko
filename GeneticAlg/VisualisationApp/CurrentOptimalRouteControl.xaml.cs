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
using RouteOptimizerLib;

namespace VisualisationApp
{
    /// <summary>
    /// Interaction logic for CurrentOptimalRouteControl.xaml
    /// </summary>
    public partial class CurrentOptimalRouteControl : UserControl
    {
        public CurrentOptimalRouteControl()
        {
            InitializeComponent();

        }
        Route lastBestRoute = null;
        public void AddNewBestRoute(Route route)
        {
            if (lastBestRoute == null) lastBestRoute = route;
            if(route.Distance < lastBestRoute.Distance) lastBestRoute = route;

            BestLabel.Content = lastBestRoute.ToString();
            BestLengthLabel.Content = lastBestRoute.Distance.ToString();
            BestRoutes.Items.Add(lastBestRoute);
        }
        public void Clear()
        {
            BestLabel.Content = "Empty";
            lastBestRoute = null;
            BestRoutes.Items.Clear();
        }
    }
}
