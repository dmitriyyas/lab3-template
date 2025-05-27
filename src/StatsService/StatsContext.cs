using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StatsService.Models;

namespace StatsService;

public partial class StatsContext : DbContext
{
    public StatsContext()
    {
    }

    public StatsContext(DbContextOptions<StatsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Stat> Stats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stats_pkey");

            entity.ToTable("stats");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.Method)
                .HasMaxLength(80)
                .HasColumnName("method");
            entity.Property(e => e.Path)
                .HasMaxLength(80)
                .HasColumnName("path");
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
