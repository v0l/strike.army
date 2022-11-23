﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StrikeArmy.Database;

#nullable disable

namespace StrikeArmy.Database.Migrations
{
    [DbContext(typeof(StrikeArmyContext))]
    [Migration("20221122225821_WithdrawConfigs")]
    partial class WithdrawConfigs
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StrikeArmy.Database.Model.AuthToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AuthToken", (string)null);
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("StrikeUserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.WithdrawConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("Max")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("Min")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("WithdrawConfig", (string)null);
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.WithdrawConfigPayment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PayeeNodePubKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Pr")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("RoutingFee")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusMessage")
                        .HasColumnType("text");

                    b.Property<Guid>("StrikeQuoteId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WithdrawConfigId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("WithdrawConfigId");

                    b.HasIndex("Created", "Status");

                    b.ToTable("WithdrawConfigPayment", (string)null);
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.AuthToken", b =>
                {
                    b.HasOne("StrikeArmy.Database.Model.User", "User")
                        .WithMany("AuthTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.WithdrawConfig", b =>
                {
                    b.HasOne("StrikeArmy.Database.Model.User", "User")
                        .WithMany("WithdrawConfigs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("StrikeArmy.Database.Model.WithdrawConfigReusable", "ConfigReusable", b1 =>
                        {
                            b1.Property<Guid>("WithdrawConfigId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Interval")
                                .HasColumnType("integer");

                            b1.Property<decimal>("Limit")
                                .HasColumnType("numeric(20,0)");

                            b1.HasKey("WithdrawConfigId");

                            b1.ToTable("WithdrawConfig");

                            b1.WithOwner("WithdrawConfig")
                                .HasForeignKey("WithdrawConfigId");

                            b1.Navigation("WithdrawConfig");
                        });

                    b.Navigation("ConfigReusable");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.WithdrawConfigPayment", b =>
                {
                    b.HasOne("StrikeArmy.Database.Model.WithdrawConfig", "WithdrawConfig")
                        .WithMany("Payments")
                        .HasForeignKey("WithdrawConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("WithdrawConfig");
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.User", b =>
                {
                    b.Navigation("AuthTokens");

                    b.Navigation("WithdrawConfigs");
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.WithdrawConfig", b =>
                {
                    b.Navigation("Payments");
                });
#pragma warning restore 612, 618
        }
    }
}
