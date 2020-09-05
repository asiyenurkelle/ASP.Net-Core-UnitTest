﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UdemyRealWordUnitTest.Web.Models
{
    public partial class UdemyUnitTestDbContext : DbContext
    {
        public UdemyUnitTestDbContext()
        {
        }

        public UdemyUnitTestDbContext(DbContextOptions<UdemyUnitTestDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Product { get; set; }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
