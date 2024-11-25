﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using DistributedTransactions.Data;

#nullable disable

namespace DistributedTransactions.Data.Migrations.HoldingsDb
{
    [DbContext(typeof(HoldingsDbContext))]
    [Migration("20241122181133_HoldingsDataMart")]
    partial class HoldingsDataMart
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DistributedTransactions.Domain.Holdings.Portfolio", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("AccountNum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Cash")
                        .HasColumnType("numeric(17, 2)");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("OpenedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Strategy")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountNum")
                        .IsUnique();

                    b.ToTable("Portfolios");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Holdings.Position", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<double>("Allocation")
                        .HasColumnType("numeric(5, 2)");

                    b.Property<string>("AssetClass")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("PortfolioId")
                        .HasColumnType("uuid");

                    b.Property<double>("Price")
                        .HasColumnType("numeric(17, 2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Value")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("numeric(17, 2)")
                        .HasComputedColumnSql("\"Quantity\" * \"Price\"", true);

                    b.HasKey("Id");

                    b.HasIndex("PortfolioId");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Holdings.Position", b =>
                {
                    b.HasOne("DistributedTransactions.Domain.Holdings.Portfolio", null)
                        .WithMany("Positions")
                        .HasForeignKey("PortfolioId");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Holdings.Portfolio", b =>
                {
                    b.Navigation("Positions");
                });
#pragma warning restore 612, 618
        }
    }
}
