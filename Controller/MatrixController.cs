using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDOJO.Controller
{
    public class MatrixController
    {
        
        public MatrixController() { }
        public static double[,] InitializeMatrix(int rows, int cols)
        {
            double[,] matrix = new double[rows, cols];

            Random rand = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rand.NextDouble() * 2 - 1; 
                }
            }
            return matrix;

        }
        public static double Sigmoid(double sum)
        {
            return 1 / (1 + Math.Exp(-sum));
        }
        public static double SigmoidDerivative(double sum)
        {
            return sum * (1 - sum);
        }
    }
}
