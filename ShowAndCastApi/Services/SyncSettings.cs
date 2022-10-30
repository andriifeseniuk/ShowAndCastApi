namespace ShowAndCastApi.Services
{
    public class SyncSettings
    {
        public int ShowsPageSize { get; set; } = 250;

        public int MinThrottlingInterval { get; set; } = 1000;

        public int MaxThrottlingInterval { get; set; } = 60000;

        public int ThrottlingRecalculateInterval { get; set; } = 600000;

        public int SyncCompletedInterval { get; set; } = 3600000;
    }
}
