namespace ShowAndCastApi.Models
{
    public class Show
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<Cast> Casts { get; set; }
    }
}
