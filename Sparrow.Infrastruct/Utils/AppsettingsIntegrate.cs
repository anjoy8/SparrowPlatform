namespace SparrowPlatform.Infrastruct.Utils
{
    public class ShujubaoApp
    {
        public string V3Key { get; set; }
    }

    public class ZXLApp
    {
        public string domain { get; set; }
        public string user { get; set; }
        public string pwd { get; set; }
        public string cid { get; set; }
        public string srt { get; set; }
        public string transactionCode { get; set; }
        public string tokenCache { get; set; }
    }

    public class AppsettingsIntegrate
    {
        public static ShujubaoApp ShujubaoApp { get; set; }
        public static ZXLApp ZXLApp { get; set; }
    }
}
