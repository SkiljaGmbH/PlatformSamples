using System.Windows.Media;

namespace EDAClient.Data
{
    internal class OEM
    {

        internal OEM()
        {
            Brand = "Giulia";
            Process = "Process";
            PrimaryColor = Brushes.Indigo.Color;
            AccentColor = Brushes.Lime.Color;
        }

        public string Brand { get; set; }

        public string Process { get; set; }

        public Color PrimaryColor { get; set; }

        public Color AccentColor { get; set; }
    }
}
