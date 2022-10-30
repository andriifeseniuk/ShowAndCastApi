namespace ShowAndCastApi.Models
{
    public class ShowSync
    {
        public int Id { get; set; }

        public long LastLoadedShowId { get; set; } = -1;

        public bool IsSyncCompleted { get; set; } = false;
    }
}
