using System.Drawing;

namespace Nicolas2422.RpiNetapi.Models
{
    public class Cpu
    {
        public string Hardware { get; set; }

        public string Model { get; set; }

        public int Frequency { get; set; }

        public decimal Temperature { get; set; }

        public decimal Usage { get; set; }

        public Cpu()
        {
            Hardware = String.Empty;
            Model = String.Empty;
            Frequency = 0;
            Temperature = 0;
            Usage = 0;
        }
    }
}