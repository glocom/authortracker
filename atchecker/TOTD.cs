using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atchecker
{
    public class TOTD
    {
        public DateOnly Day { get; set; }
        public string MapName { get; set; }
        public string Author { get; set; }
        public int NrOfAts { get; set; }
        public TimeOnly AtTime { get; set; }
        public List<Player> Players { get; set; }
    }
}
