using Android.Media;
using Plugin.BluetoothClassic.Abstractions;

namespace ToothScan.Models
{
    public class Connections
    {
        public static IBluetoothManagedConnection CurrentBluetoothConnection { get; internal set; }
        public static AudioManager sound_lvl { get; set; }
        public static string searcher_text_transmission { get; set; }
        public static int start_search_index { get; set; }
    }
}
