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
using RouteOptimizerLib;

namespace VisualisationApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        RouteOptimizer routeOptimizer;
        bool AlgInProcess = false;
        public void CreateAlg()
        {
            this.CurrentBestTextControl.Clear();
            epochCount = Convert.ToInt32(EpochCountTextBox.Text);
            int popSize = Convert.ToInt32(PopulationSizeTextBox.Text);
            double[,] data = this.MatrixControl.GetValues();
            if (data == null) throw new Exception("Empty matrix");
            List<int> selectedCities = new List<int>();
            for (int i = 0; i < data.GetLength(0); i++) selectedCities.Add(i);
            routeOptimizer = new RouteOptimizer(data.GetLength(0), data, selectedCities);
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlgInProcess)return;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            try
            {
                this.CurrentBestTextControl.Clear();
                CreateAlg();   
                StartAlgorithm();
                AlgInProcess = true;
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        int currentEpoch = 0;
        int epochCount = 0;
        private CancellationTokenSource _cancellationTokenSource;

        private async void StartAlgorithm()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                // Генетический алгоритм
                while (!token.IsCancellationRequested)
                {
                    // Выполнение итераций алгоритма
                    var bestPath = routeOptimizer.Calculate_Parallel();
                    currentEpoch++;
                    // Обновление UI с использованием Dispatcher.Invoke
                    
                    if(currentEpoch % 1 == 0)
                    {
                        visualisation(bestPath);
                    }
                    
                }
            }, token);
        }
        public async void visualisation(Route best)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.CurrentBestTextControl.AddNewBestRoute(best);
            });
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if(!AlgInProcess) return;
            StopAlgorithm();
            AlgInProcess = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void StopAlgorithm()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}