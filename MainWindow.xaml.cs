using ArduinoDOJO.Controller;
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
using System.Diagnostics;
using ArduinoDOJO.Model;
using System.Data;
using Newtonsoft.Json;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using System.Globalization;
using SpreadsheetGear;
using ArduinoMAZE.Controller;

namespace ArduinoDOJO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MatrixController matrixController;
        SQLController sQLController;
        JsonFilter jsonFilter;
        AIController aiController;

        double[,] GLOBALweight_ih;
        double[,] GLOBALweight_ho;
        int INPUT_SIZE, HIDDEN_SIZE, OUTPUT_SIZE, EPOCH;
        double LEARNING_RATE;

        List<(object[,], object[,])> matrix = new List<(object[,], object[,])>();

        public MainWindow()
        {
            InitializeComponent();
            matrixController = new MatrixController();
            sQLController = new SQLController();
            InitializeCB_Models();
        }
        private async void InitializeCB_Models()
        {
            CB_Models.ItemsSource = await sQLController.GetNomsModeles();
        }

        private void InputSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TB_InputCurrent != null)
            {
                TB_InputCurrent.Text = InputSlider.Value.ToString();
            }
        }
        private void HiddenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TB_HiddenCurrent != null)
            {
                TB_HiddenCurrent.Text = HiddenSlider.Value.ToString();
            }
        }
        private void LearningRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TB_LearningRateCurrent != null)
            {
                TB_LearningRateCurrent.Text = LearningRateSlider.Value.ToString();
            }
        }
        private void EpochSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TB_EpochCurrent != null)
            {
                TB_EpochCurrent.Text = EpochSlider.Value.ToString();
            }
        }
        private async void BTN_Train_Click(object sender, RoutedEventArgs e)
        {
            BTN_Predict.IsEnabled = true;
            BTN_Save.IsEnabled = true;

            INPUT_SIZE = (int)InputSlider.Value;
            HIDDEN_SIZE = (int)HiddenSlider.Value;
            LEARNING_RATE = (double)LearningRateSlider.Value;
            OUTPUT_SIZE = 1;
            EPOCH = (int)EpochSlider.Value;

            if (CB_mode.IsChecked == false)
            {
                matrix = await sQLController.LoadTraningDataFromExcelFile();
            }
            else
            {
                matrix = await sQLController.getEntrainement();
            }

            if (matrix == null)
            {
                MessageBox.Show("Error loading data");
                return;
            }

            TrainingGrid.ItemsSource = null;
            TrainingGrid.Items.Clear();
            List<DataModel> dataList = new List<DataModel>();

            for (int j = 0; j < matrix[0].Item1.GetLength(0); j++)
            {
                dataList.Add(new DataModel
                {
                    X = Convert.ToInt32(matrix[0].Item1[j, 0]),
                    Y = Convert.ToInt32(matrix[0].Item1[j, 1]),
                    Esc = Convert.ToInt32(matrix[0].Item1[j, 2]),
                    Up = Convert.ToInt32(matrix[0].Item1[j, 3]),
                    Down = Convert.ToInt32(matrix[0].Item1[j, 4]),
                    Right = Convert.ToInt32(matrix[0].Item1[j, 5]),
                    Left = Convert.ToInt32(matrix[0].Item1[j, 6]),
                });
            }

            LoadingGif.Visibility = Visibility.Visible;
            MessageBox.Show("sup");
            await Task.Run(async () =>
            {
                IAModel iaModel = await aiController.Training(matrix[0].Item1, matrix[0].Item2, INPUT_SIZE, HIDDEN_SIZE, EPOCH, OUTPUT_SIZE, LEARNING_RATE);

                GLOBALweight_ho = iaModel.weights_ho;
                GLOBALweight_ih = iaModel.weights_ih;

                List<DataModel> emptyData = new List<DataModel>();

                for (int i = 0; i < matrix[0].Item1.GetLength(0); i++)
                {
                    emptyData.Add(new DataModel
                    {
                        Id = i + 1,
                        X = 0,
                        Y = 0,
                        Esc = 0,
                        Up = 0,
                        Down = 0,
                        Right = 0,
                        Left = 0,
                        Tag = 0
                    });
                }

                TrainingGrid.ItemsSource = null;
                TrainingGrid.Items.Clear();
                TrainingGrid.ItemsSource = emptyData;
                LoadingGif.Visibility = Visibility.Hidden;

            });
        }

        private async void BTN_Predict_Click(object sender, RoutedEventArgs e)
        {
            INPUT_SIZE = (int)InputSlider.Value;
            HIDDEN_SIZE = (int)HiddenSlider.Value;

            int rowCount = TrainingGrid.Items.Count;
            int columnCount = TrainingGrid.Columns.Count;

            int[,] trainingMatrix = new int[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                var row = TrainingGrid.Items[i];
                for (int j = 0; j < columnCount; j++)
                {
                    var cellContent = TrainingGrid.Columns[j].GetCellContent(row);

                    if (cellContent is TextBlock textBlock)
                    {
                        int value;
                        if (int.TryParse(textBlock.Text, out value))
                        {
                            trainingMatrix[i, j] = value;
                        }
                        else
                        {
                            trainingMatrix[i, j] = 0; // or handle the error as needed
                        }
                    }
                    else
                    {
                        trainingMatrix[i, j] = int.Parse(cellContent?.ToString());
                    }
                }
            }

            LoadingGif.Visibility = Visibility.Visible;

            await Task.Run(async () =>
            {
                List<DataModel> predictDataList = await aiController.Predict(trainingMatrix, INPUT_SIZE, HIDDEN_SIZE, GLOBALweight_ih, GLOBALweight_ho);
                PredictGrid.ItemsSource = null;
                PredictGrid.Items.Clear();
                PredictGrid.ItemsSource = predictDataList;
                LoadingGif.Visibility = Visibility.Hidden;

            });
            
        }
        private async void BTN_Load_Click(object sender, RoutedEventArgs e)
        {
            BTN_Load.IsEnabled = true;
            BTN_Predict.IsEnabled = true;
            BTN_Save.IsEnabled = true;

            if (CB_Models.SelectedItem == null)
            {
                MessageBox.Show("Please choose a model");
                return;
            }

            var reponse = await sQLController.getModelByName(CB_Models.SelectedItem.ToString());

            if (reponse != null)
            {
                GLOBALweight_ih = jsonFilter.FilterMatrixString(reponse.weights_ih);
                GLOBALweight_ho = jsonFilter.FilterMatrixString(reponse.weights_ho);

                int weights_Size = jsonFilter.GetWeightSize();

                List<DataModel> emptyData = new List<DataModel>();

                for (int i = 0; i < weights_Size; i++)
                {
                    emptyData.Add(new DataModel
                    {
                        Id = i + 1,
                        X = 0,
                        Y = 0,
                        Esc = 0,
                        Up = 0,
                        Down = 0,
                        Left = 0,
                        Right = 0,
                        Tag = 0
                    });
                }

                TrainingGrid.ItemsSource = null;
                TrainingGrid.Items.Clear();
                TrainingGrid.ItemsSource = emptyData;
                MessageBox.Show("Model loaded");
            }
        }

        private async void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("yyyy-MM-dd_HH:mm:ss");
            MessageBox.Show(formattedDate);
            var weights_ihConvert = JsonConvert.SerializeObject(GLOBALweight_ih);
            var weights_hoConvert = JsonConvert.SerializeObject(GLOBALweight_ho);
            var data = new
            {
                model_name = $"Model_{date}",
                weights_ih = weights_ihConvert,
                weights_ho = weights_hoConvert
            };

            var jsonData = JsonConvert.SerializeObject(data);
            sQLController.SaveDataAsync($"Model_{date}", GLOBALweight_ih, GLOBALweight_ho);

        }
    }
}