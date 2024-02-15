using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atchecker
{
    public class Player
    {
        public string PlayerUid { get; set; }
        public TimeOnly PbTime { get; set; }
        public bool HasAt { get; set; }
    }
}
