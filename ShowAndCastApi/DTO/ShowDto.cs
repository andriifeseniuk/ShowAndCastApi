namespace ShowAndCastApi.DTO
{
    public class ShowDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<PersonDto> Cast { get; set; }
    }
}
