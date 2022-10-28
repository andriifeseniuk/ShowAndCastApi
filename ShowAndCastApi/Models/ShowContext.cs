﻿using Microsoft.EntityFrameworkCore;

namespace ShowAndCastApi.Models
{
    public class ShowContext : DbContext
    {
        public ShowContext(DbContextOptions<ShowContext> options)
            : base(options)
        {
        }

        public DbSet<Show> Shows { get; set; }
        public DbSet<Cast> Casts { get; set; }
    }
}
