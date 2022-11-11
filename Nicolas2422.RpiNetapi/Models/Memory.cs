using System.Drawing;

namespace Nicolas2422.RpiNetapi.Models
{
    public class Memory
    {
        public int Total { get; set; }

        public int Used { get; set; }

        public int Free { get; set; }

        public decimal Usage { get; set; }

        public Memory()
        {
            Total = 0;
            Used = 0;
            Free = 0;
            Usage = 0;
        }
    }
}