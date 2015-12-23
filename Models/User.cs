using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMVC.Models
{
    public class User
    {
        public string id { get; set; }
        public string pass { get; set; } // this is never used.
        public string name { get; set; }
    }
}
