using System.ComponentModel.DataAnnotations.Schema;

namespace ShowAndCastApi.Models
{
    public class Cast
    {
        public long Id { get; set; }

        public long ShowId { get; set; }

        public long PersonId { get; set; }

        public virtual Person Person { get; set; }

    }
}
