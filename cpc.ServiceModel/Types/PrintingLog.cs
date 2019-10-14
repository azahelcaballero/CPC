using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpc.ServiceModel.Types
{
    public class PrintingLog
    {
        [AutoIncrement]
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public String User { get; set;  }
        public Double Pages { get; set; }
        public String DocumentName { get; set; }
        public String Client { get; set; }

        public String Language { get; set; }
        public Double Height { get; set; }
        public Double Width { get; set; }

        public bool duplex { get; set; }
        public bool grayscale { get; set; }
        public String Size { get; set; }





    }
}
