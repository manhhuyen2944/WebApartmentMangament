using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebApartmentMangament.Models
{
    public partial class QUANLYCHUNGCUContext : DbContext
    {
        public QUANLYCHUNGCUContext()
        {
        }

        public QUANLYCHUNGCUContext(DbContextOptions<QUANLYCHUNGCUContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
      //  public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<Apartment> Apartments { get; set; } = null!;
        public virtual DbSet<ApartmentService> ApartmentServices { get; set; } = null!;
        public virtual DbSet<Building> Buildings { get; set; } = null!;
        public virtual DbSet<Contract> Contracts { get; set; } = null!;
        public virtual DbSet<ElectricMeter> ElectricMeters { get; set; } = null!;
        public virtual DbSet<History> Histories { get; set; } = null!;
        public virtual DbSet<InFo> InFos { get; set; } = null!;
        public virtual DbSet<News> News { get; set; } = null!;
        public virtual DbSet<Relationship> Relationships { get; set; } = null!;
        public virtual DbSet<ResidentsRequired> ResidentsRequireds { get; set; } = null!;
        public virtual DbSet<Revenue> Revenues { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Service> Services { get; set; } = null!;
        public virtual DbSet<WaterMeter> WaterMeters { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-VC92P42\\SQLEXPRESS;Initial Catalog=QUANLYCHUNGCU;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.HasIndex(e => e.ApartmentId, "IX_Account_ApartmentId");

                entity.HasIndex(e => e.InfoId, "IX_Account_InfoId");

                entity.HasIndex(e => e.RelationshipId, "IX_Account_RelationshipId");

                entity.HasIndex(e => e.RoleId, "IX_Account_RoleId");

                entity.Property(e => e.Avartar)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_Account_Apartment").OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Info)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.InfoId)
                    .HasConstraintName("FK_Account_InFo").OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Relationship)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RelationshipId).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_Account_Role").OnDelete(DeleteBehavior.Cascade);
            });

            //modelBuilder.Entity<Address>(entity =>
            //{
            //    entity.ToTable("Address");

            //    entity.Property(e => e.Ward).HasColumnName("ward");
            //});

            modelBuilder.Entity<Apartment>(entity =>
            {
                entity.ToTable("Apartment");

                entity.HasIndex(e => e.BuildingId, "IX_Apartment_BuildingId");

                entity.Property(e => e.ApartmentCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Apartments)
                    .HasForeignKey(d => d.BuildingId)
                    .HasConstraintName("FK_CanHo_ChungCu").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApartmentService>(entity =>
            {
                entity.HasKey(e => new { e.ApartmentId, e.ServiceId });

                entity.ToTable("Apartment_Service");

                entity.HasIndex(e => e.ServiceId, "IX_Apartment_Service_ServiceId");

                entity.Property(e => e.EndDay).HasColumnType("date");

                entity.Property(e => e.StartDay).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.ApartmentServices)
                    .HasForeignKey(d => d.ApartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Apartment_Service_Apartment");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ApartmentServices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Apartment_Service_Service");
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("Building");
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("Contract");

                entity.HasIndex(e => e.ApartmentId, "IX_Contract_ApartmentId");

                entity.Property(e => e.Deposit).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EndDay).HasColumnType("date");

                entity.Property(e => e.MonthlyRent)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("Monthly_rent");

                entity.Property(e => e.StartDay).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.Contracts)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_Contract_Apartment");
            });

            modelBuilder.Entity<ElectricMeter>(entity =>
            {
                entity.ToTable("ElectricMeter");

                entity.HasIndex(e => e.ApartmentId, "IX_ElectricMeter_ApartmentId");

                entity.Property(e => e.Code).IsUnicode(false);

                entity.Property(e => e.DeadingDate).HasColumnType("date");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RegistrationDate).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.ElectricMeters)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_ElectricMeter_Apartment");
            });

            modelBuilder.Entity<History>(entity =>
            {
                entity.ToTable("History");

                entity.HasIndex(e => e.AccountId, "IX_History_AccountId");

                entity.Property(e => e.Day).HasColumnType("date");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Histories)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_History_Account");
            });

            modelBuilder.Entity<InFo>(entity =>
            {
                entity.ToTable("InFo");

                //entity.HasIndex(e => e.AddressId, "IX_InFo_AddressId");

                entity.Property(e => e.BirthDay)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.CmndCccd)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("CMND_CCCD");

                //entity.HasOne(d => d.Address)
                //    .WithMany(p => p.InFos)
                //    .HasForeignKey(d => d.AddressId)
                //    .HasConstraintName("FK_InFo_Address").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.Property(e => e.CreateDay).HasColumnType("date");

                entity.Property(e => e.Description).HasColumnName("description");
            });

            modelBuilder.Entity<ResidentsRequired>(entity =>
            {
                entity.HasKey(e => e.RequestId);

                entity.ToTable("ResidentsRequired");

                entity.HasIndex(e => e.ApartmentId, "IX_ResidentsRequired_ApartmentId");

                entity.Property(e => e.CreateDay).HasColumnType("date");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.FixDay).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.ResidentsRequireds)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_ResidentsRequired_Apartment").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Revenue>(entity =>
            {
                entity.ToTable("Revenue");

                entity.HasIndex(e => e.ApartmentId, "IX_Revenue_ApartmentId");

                entity.Property(e => e.CodeVoucher).IsUnicode(false);

                entity.Property(e => e.DayCreat).HasColumnType("date");

                entity.Property(e => e.DayPay).HasColumnType("date");

                entity.Property(e => e.Debt).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Pay).HasColumnType("decimal(18, 0)");


                entity.Property(e => e.ServiceFee).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TotalMoney).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.Revenues)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_Revenue_Apartment").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.ServiceFee).HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<WaterMeter>(entity =>
            {
                entity.ToTable("WaterMeter");

                entity.HasIndex(e => e.ApartmentId, "IX_WaterMeter_ApartmentId");

                entity.Property(e => e.Code).IsUnicode(false);

                entity.Property(e => e.DeadingDate).HasColumnType("date");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RegistrationDate).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.WaterMeters)
                    .HasForeignKey(d => d.ApartmentId)
                    .HasConstraintName("FK_WaterMeter_Apartment").OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
