﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using DistributedTransactions.Data;

#nullable disable

namespace DistributedTransactions.Data.Migrations.TradesDb
{
    [DbContext(typeof(TradesDbContext))]
    partial class TradesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DistributedTransactions.Domain.Trades.AdHocTrade", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("AccountNum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ActivityType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<int?>("Amount")
                        .HasColumnType("integer");

                    b.Property<string>("AssetClass")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BlockOrderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("BlockOrderSeqNum")
                        .HasColumnType("integer");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Destination")
                        .HasColumnType("integer");

                    b.Property<int>("Direction")
                        .HasColumnType("integer");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid>("PositionId")
                        .HasColumnType("uuid");

                    b.Property<double?>("Price")
                        .HasColumnType("double precision");

                    b.Property<int>("Restriction")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BlockOrderCode", "BlockOrderSeqNum", "Date")
                        .IsUnique();

                    b.ToTable("AdHocTrades");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Trades.BustedTrade", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<int>("ActivityType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<int?>("Amount")
                        .HasColumnType("integer");

                    b.Property<string>("AssetClass")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BlockOrderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("BlockOrderSeqNum")
                        .HasColumnType("integer");

                    b.Property<int>("CancelledAmount")
                        .HasColumnType("integer");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Destination")
                        .HasColumnType("integer");

                    b.Property<int>("Direction")
                        .HasColumnType("integer");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<double?>("Price")
                        .HasColumnType("double precision");

                    b.Property<int>("Restriction")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BlockOrderCode", "BlockOrderSeqNum", "Date")
                        .IsUnique();

                    b.ToTable("BustedTrades");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Trades.ExecutedTrade", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<int>("ActivityType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(2);

                    b.Property<int?>("Amount")
                        .HasColumnType("integer");

                    b.Property<string>("AssetClass")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BlockOrderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("BlockOrderSeqNum")
                        .HasColumnType("integer");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Destination")
                        .HasColumnType("integer");

                    b.Property<int>("Direction")
                        .HasColumnType("integer");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<double?>("Price")
                        .HasColumnType("double precision");

                    b.Property<int>("Restriction")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BlockOrderCode", "BlockOrderSeqNum", "Date")
                        .IsUnique();

                    b.ToTable("ExecutedTrades");
                });

            modelBuilder.Entity("DistributedTransactions.Domain.Trades.ReplacedTrade", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<int>("ActivityType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(3);

                    b.Property<int?>("Amount")
                        .HasColumnType("integer");

                    b.Property<string>("AssetClass")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BlockOrderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("BlockOrderSeqNum")
                        .HasColumnType("integer");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Destination")
                        .HasColumnType("integer");

                    b.Property<int>("Direction")
                        .HasColumnType("integer");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("system");

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<int?>("NewAmount")
                        .HasColumnType("integer");

                    b.Property<int?>("NewDestination")
                        .HasColumnType("integer");

                    b.Property<int?>("NewRestriction")
                        .HasColumnType("integer");

                    b.Property<int?>("NewType")
                        .HasColumnType("integer");

                    b.Property<double?>("Price")
                        .HasColumnType("double precision");

                    b.Property<int>("Restriction")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BlockOrderCode", "BlockOrderSeqNum", "Date")
                        .IsUnique();

                    b.ToTable("ReplacedTrades");
                });
#pragma warning restore 612, 618
        }
    }
}
