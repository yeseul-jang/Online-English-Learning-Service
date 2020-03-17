using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public partial class API_Lab3Context : DbContext
    {
        public API_Lab3Context()
        {
        }

        public API_Lab3Context(DbContextOptions<API_Lab3Context> options)
            : base(options)
        {
        }

        public virtual DbSet<TimeSlot> TimeSlot { get; set; }
        public virtual DbSet<User> User { get; set; }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=sqlserver.c3aq4vfkg0ih.ca-central-1.rds.amazonaws.com,1433;database=API_Lab3;User ID=admin;Password=11633477;Connect Timeout=120;");
            }
        }
        */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.Property(e => e.TimeSlotID)
                    .HasColumnName("timeSlotID")
                    .ValueGeneratedNever();

                entity.Property(e => e.StrTimeSlot)
                    .HasColumnName("timeSlot")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserID).HasColumnName("userID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TimeSlots)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSlot_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserID)
                    .HasColumnName("userID")
                    .ValueGeneratedNever();

                entity.Property(e => e.EmailID)
                    .IsRequired()
                    .HasColumnName("emailID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("firstName")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("lastName")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });
        }
    }
}
