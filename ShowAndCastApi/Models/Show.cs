using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowAndCastApi.Models
{
    public class Show
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Cast> Casts { get; set; }
    }
}
