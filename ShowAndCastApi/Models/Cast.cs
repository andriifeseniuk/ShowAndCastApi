namespace ShowAndCastApi.Models
{
    public class Cast
    {
        public long Id { get; set; }
        public long ShowId { get; set; }
        public string? Name { get; set; }
        public DateTime Birthday { get; set; }
    }
}
