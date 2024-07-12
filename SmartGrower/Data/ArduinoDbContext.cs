using SmartGrower.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NuGet.Protocol.Plugins;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace SmartGrower.Data
{
    public class ArduinoDbContext : DbContext
    {
        public ArduinoDbContext(DbContextOptions<ArduinoDbContext> options) : base(options)
        {
        }
    
        public DbSet<TipoMedicao> TipoMedicoes { get; set; }
        public DbSet<Medicao> Medicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<TipoMedicao>(d =>
            { d.Property(e => e.TipoMedicaoId).ValueGeneratedNever(); });


            modelBuilder.Entity <Medicao>(d =>
            { d.Property(e => e.MedicaoId).ValueGeneratedOnAdd(); });

            modelBuilder.Entity<Medicao>()
                .HasOne(d => d.TipoMedicao)
                .WithMany(m => m.Medicoes)
                .OnDelete(DeleteBehavior.Restrict);

        }


    }
}
