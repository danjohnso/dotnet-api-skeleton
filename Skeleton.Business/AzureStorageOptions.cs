namespace Skeleton.Business
{
    public class AzureStorageOptions
    {
        public required string AccountName { get; set; }
        public required int SASLifetimeMinutes { get; set; }
        public required Uri ServiceUri { get; set; }
    }
}
