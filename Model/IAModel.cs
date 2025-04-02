using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDOJO.Model
{
    public class IAModel
    {
        public List<DataModel> dataModel { get; set; }
        public double[,] weights_ih { get; set; }
        public double[,] weights_ho { get; set; }
    }
}