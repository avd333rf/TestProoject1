using System;
using System.Collections.Generic;
using System.Linq;
using CitizenDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace CitizenDatabase.Database
{
    public class CitizenDatabase : DbContext
    {
        public CitizenDatabase(DbContextOptions<CitizenDatabase> options)
            : base(options)
        {
        }

        public DbSet<Citizen> Citizens { get; set; }

        /// <summary>
        /// Create default DB and create test records
        /// </summary>
        public void Initialize()
        {
            this.Database.EnsureCreated();

            if (this.Citizens.Any())
            {
                return;
            }

            GenerateData();
        }

        private void GenerateData()
        {
            var fNames = new string[] { "Ivan", "Alex", "Anna", "Pyotr", "Nina", "Lena", "Igor", "Oleg", "Katya" };
            var mNames = new string[] { "Petrovich", "Ivanoovich", "Dmitrievich" };
            var lNames = new string[] { "Ivanov", "Petroov", "Pushkin", "Tolstoy" };

            var rnd = new Random();
            var rndDate = new DateTime(1900, 1, 1);

            var citizens = new List<Citizen>(1000000);
            for (int i = 0; i < 5000; i++)
            {
                var c = new Citizen();
                c.FullName = $"{lNames[rnd.Next(0, lNames.Length)]} {fNames[rnd.Next(0, fNames.Length)]} {mNames[rnd.Next(0, mNames.Length)]}";
                c.Snils = $"{rnd.Next(0, 999):000}-{rnd.Next(0, 999):000}-{rnd.Next(0, 999):000} 00";
                c.Inn = rnd.Next(int.MaxValue).ToString("000000000000");
                c.BirthDate = rndDate.AddDays(rnd.Next((DateTime.Today - rndDate).Days));
                c.DeathDate = rnd.NextDouble() < 0.8d ? null : c.BirthDate.AddDays(rnd.Next((DateTime.Today - c.BirthDate).Days));
                citizens.Add(c);
            }

            this.Citizens.AddRange(citizens);
            this.SaveChanges();
        }
    }
}