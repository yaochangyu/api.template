using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.DB;

public partial class MemberDbContext : DbContext
{
    public MemberDbContext(DbContextOptions<MemberDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Member> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Member_pk");

            entity.ToTable("Member");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ChangedBy).HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.SequenceId).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
