using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowAndCastApi.Models
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime Birthday { get; set; }
    }
}
