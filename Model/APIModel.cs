using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDOJO.Model
{
    public class APIModel
    {
        public int id { get; set; }
        public string model_name { get; set; }
        public string weights_ih { get; set; }
        public string weights_ho { get; set; }
    }
}
