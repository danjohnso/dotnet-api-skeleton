namespace Skeleton.Email
{
    public class SmtpOptions
    {
        public string FolderPath { get; set; } = "";
        public string FailedSendFolderPath { get; set; } = "";
        public string ServerAddress { get; set; } = "";
        public int Port { get; set; }
        public bool IsSsl { get; set; }
        public bool IsProduction { get; set; }
    }
}
