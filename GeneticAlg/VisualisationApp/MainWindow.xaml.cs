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
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using System.IO;

namespace VisualisationApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExperimentFilesHolder experimentFilesHolder;

        public MainWindow()
        {
            InitializeComponent();
            experimentFilesHolder = new ExperimentFilesHolder();
            experimentFilesHolder.Load();
            updateOpenOptions();
            BuildButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
        }
        RouteOptimizer routeOptimizer;
        bool AlgInProcess = false;
        public void CreateAlg()
        {
               // if (routeOptimizer.CurrentPopulation.Count > 0) return;
                this.CurrentBestTextControl.Clear();
                int popSize = Convert.ToInt32(PopulationSizeTextBox.Text);
                double[,] data = this.MatrixControl.GetValues();
                if (data == null) throw new Exception("Empty matrix");
                List<int> selectedCities = new List<int>();
                for (int i = 0; i < data.GetLength(0); i++) selectedCities.Add(i);
                routeOptimizer = new RouteOptimizer(data.GetLength(0), data, selectedCities);
                routeOptimizer.Calculate_Parallel(popSize, 1);

            
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlgInProcess) return;
            BuildButton.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StartAlgorithm();
            AlgInProcess = true;
        }
        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlgInProcess) return;
            BuildButton.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            try
            {
                this.CurrentBestTextControl.Clear();
                CreateAlg();
            }
            catch (Exception ex)
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
            Task longRunningTask = Task //fixed
                .Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    // Выполнение итераций алгоритма
                    var bestPath = routeOptimizer.NextIteration_Parallel();
                    currentEpoch++;
                    // Обновление UI с использованием Dispatcher.Invoke

                    if (currentEpoch % 1 == 0)
                    {
                        Visualisation(bestPath);
                    }

                }
            }, token);
                
        }
        public async void Visualisation(Route best)
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
            BuildButton.IsEnabled = true;
        }

        private void StopAlgorithm()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }


        #region File saving/opening 
        private string SavePopulation(Population pop)
        {
            return JsonSerializer.Serialize(pop);
        }
        private Population LoadPopulation(string pop)
        {
            return JsonSerializer.Deserialize<Population>(pop) as Population;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
            {
                if (experimentFilesHolder.Contains(sfd.FileName))
                {
                    MessageBox.Show("Expiremnt with this name already exists");
                    return;
                }
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.WriteLine(routeOptimizer.Save());
                }
                experimentFilesHolder.AddExperiment(routeOptimizer, sfd.FileName);
                updateOpenOptions();
            }
        }
        private void LoadRouteOptimizer(RouteOptimizer routeOptimizer)
        {
            if (routeOptimizer == null) return;
            this.routeOptimizer = routeOptimizer;
            this.MatrixControl.LoadMatrix(routeOptimizer.Matrix);
            this.PopulationSizeTextBox.Text = routeOptimizer.CurrentPopulation.Count.ToString();
            this.CurrentBestTextControl.AddNewBestRoute(routeOptimizer.getBestRoute(routeOptimizer.CurrentPopulation));
            BuildButton.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
        private void updateOpenOptions()
        {
            OpenMenuControl.Items.Clear();
            foreach (string name in experimentFilesHolder.Names)
            {
                MenuItem newMenuItem = new MenuItem { Header = name};
                newMenuItem.Click += (o, e) => { LoadRouteOptimizer(experimentFilesHolder.getRouteOptimizer(name)); };
                OpenMenuControl.Items.Add(newMenuItem);
            }
        }

        #endregion

        
    }
}