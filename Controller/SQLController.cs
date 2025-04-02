using ArduinoDOJO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.RightsManagement;
using System.Windows.Media.Animation;
using System.Windows;
using Microsoft.Win32;

namespace ArduinoDOJO.Controller
{
    public class SQLController
    {
        HttpClient client = new HttpClient();
        public SQLController()
        {
            client.BaseAddress = new Uri("https://guerraz.alwaysdata.net");
        }

        public async Task SaveDataAsync(string name, double[,] GLOBALweight_ih, double[,] GLOBALweight_ho)
        {
            var data = new
            {
                model_name = name,
                weights_ih = GLOBALweight_ih,
                weights_ho = GLOBALweight_ho
            };

            var jsonData = JsonConvert.SerializeObject(data);

            MessageBox.Show(jsonData);

            using (var content = new StringContent(jsonData, Encoding.UTF8, "application/json"))
            {
                var response = await client.PostAsync("/AddModel", content);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Data saved successfully");
                }
                else
                {
                    Console.WriteLine("Error saving data");
                }
            }
        }
        public async Task<double[,]> LoadTraningDataFromDB()
        {
            var response = await client.GetAsync("/GetModel");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                double[,] matrix = JsonConvert.DeserializeObject<double[,]>(content);
                return matrix;
            }
            else
            {
                Console.WriteLine("Error loading data");
                return null;
            }
        }

        public async Task<List<string>> GetNomsModeles()
        {

            var response = await client.GetAsync("/getAllModelNames");
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                List<string> nomsModeles = JsonConvert.DeserializeObject<List<string>>(content);
                return nomsModeles;
            }
            else
            {
                return null;
            }
        }

        public async Task<APIModel> getModelByName(string modelName)
        {
            var response = await client.GetAsync($"/getModelByName/{modelName}");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                APIModel model = JsonConvert.DeserializeObject<APIModel>(content);
                return model;
            }
            else
            {
                return null;

            }
        }

        public async Task<List<(object[,], object[,])>> LoadTraningDataFromExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx";
            openFileDialog.Multiselect = false;
            openFileDialog.ShowDialog();
            string filePath = openFileDialog.FileName;
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            SpreadsheetGear.IWorkbook workbook = SpreadsheetGear.Factory.GetWorkbook(filePath);
            object[,] matrixX = (object[,])workbook.Worksheets[0].Cells["A2:G81"].Value;
            object[,] matrixY = (object[,])workbook.Worksheets[0].Cells["H2:H81"].Value;

            return new List<(object[,], object[,])> { (matrixX, matrixY) };
        }

        public async Task<List<APIModel>> getAllModelNames()
        {
            var response = await client.GetAsync("/getAllModels");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                List<APIModel> models = JsonConvert.DeserializeObject<List<APIModel>>(content);
                return models;
            }
            else
            {
                Console.WriteLine("Error loading data");
                return null;
            }
        }
        public async Task<APIModel> getModelByName()
        {
            var response = await client.GetAsync("/getModelByName");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                APIModel model = JsonConvert.DeserializeObject<APIModel>(content);
                return model;
            }
            else
            {
                Console.WriteLine("Error loading data");
                return null;
            }
        }
        public async Task<List<(object[,], object[,])>> getEntrainement()
        {
            var response = await client.GetAsync("/getAllEntrainements");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                List<DataModel> dataList = JsonConvert.DeserializeObject<List<DataModel>>(content);
                var matrixX = new object[dataList.Count, 7];
                var matrixY = new object[dataList.Count, 1];

                for (int i = 0; i < dataList.Count; i++)
                {
                    matrixX[i, 0] = dataList[i].X;
                    matrixX[i, 1] = dataList[i].Y;
                    matrixX[i, 2] = dataList[i].Esc;
                    matrixX[i, 3] = dataList[i].Up;
                    matrixX[i, 4] = dataList[i].Down;
                    matrixX[i, 5] = dataList[i].Right;
                    matrixX[i, 6] = dataList[i].Left;

                    matrixY[i, 0] = dataList[i].Tag;
                }

                return new List<(object[,], object[,])> { (matrixX, matrixY) };
            }
            else
            {
                Console.WriteLine("Error loading data");
                return null;
            }
        }
    }
}
