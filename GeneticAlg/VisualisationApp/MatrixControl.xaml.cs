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

namespace VisualisationApp
{
    /// <summary>
    /// Interaction logic for MatrixControl.xaml
    /// </summary>
    public partial class MatrixControl : UserControl
    {
        public MatrixControl()
        {
            InitializeComponent();
            Button_Click(null, null);
        }
        Random random = new Random();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int count = Convert.ToInt32(CitiesNumberTextBox.Text);
            CreateMatrixVisualisation(count);
        }
        TextBox[,] textBoxes;
        double[,] data;
        private void CreateMatrixVisualisation(int count)
        {
            MainGrid.Children.Clear();
            data = randomMatrix(count);
            textBoxes = null;
            if (count > 7) // not to display too large matrix
            {
                MainGrid.Children.Add(new Label() { Content = "Too large to display" });
                return;
            }
            textBoxes = new TextBox[count, count];
            for (int i = 0; i < count; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
            }
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    textBoxes[i, j] = new TextBox() { Text = $"{data[i, j]}" };
                    MainGrid.Children.Add(textBoxes[i, j]);
                    textBoxes[i, j].TextChanged += TextChanged;
                    Grid.SetColumn(textBoxes[i, j], j);
                    Grid.SetRow(textBoxes[i, j], i);
                }
            }
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            int row = Grid.GetRow((TextBox)sender);
            int column = Grid.GetColumn((TextBox)sender);
            try
            {
                double newVal = Convert.ToDouble(((TextBox)sender).Text);
                textBoxes[column, row].Text = newVal.ToString();
            }
            catch
            {
                MessageBox.Show("Fill only numbers");
            }


        }
        private double[,] randomMatrix(int size)
        {
            double[,] arr = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                arr[i, i] = 0;
                for (int j = 0; j < i; j++)
                {
                    arr[i, j] = arr[j, i] = random.Next(1, 100);

                }

            }
            return arr;
        }

        public double[,] GetValues()
        {
            if (textBoxes == null) return data;
            double[,] res = new double[textBoxes.GetLength(0), textBoxes.GetLength(1)];
            for (int i = 0; i < res.GetLength(0); i++)
            {
                for (int j = 0; j < res.GetLength(1); j++)
                {
                    try
                    {

                        res[i, j] = Convert.ToDouble(textBoxes[i, j].Text);
                    }
                    catch {
                        MessageBox.Show($"Wrong value in position ({i},{j})");
                        return null;
                    }
                }
            }

            return res;
        }

    }
}
