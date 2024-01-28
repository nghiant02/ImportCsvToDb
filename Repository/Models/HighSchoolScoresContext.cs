﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace Repository.Models
{
    public partial class HighSchoolScoresContext : DbContext
    {
        public HighSchoolScoresContext()
        {
        }

        public HighSchoolScoresContext(DbContextOptions<HighSchoolScoresContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Score> Scores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Replace "YourConnectionString" with your actual connection string
            string connectionString = "Data Source=LEO\\SQLEXPRESS;Initial Catalog=HighSchoolScores;User ID=sa;Password=123456;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(); // If needed, add other options
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Score>(entity =>
            {
                entity.HasKey(e => e.SBD).HasName("PK__Scores__CA19001653F8674B");

                entity.Property(e => e.SBD).ValueGeneratedNever();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
