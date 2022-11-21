﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StrikeArmy.Database;

#nullable disable

namespace StrikeArmy.Database.Migrations
{
    [DbContext(typeof(StrikeArmyContext))]
    partial class StrikeArmyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<decimal?>("Max")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("Min")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("WithdrawConfig", (string)null);
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

                    b.Navigation("User");
                });

            modelBuilder.Entity("StrikeArmy.Database.Model.User", b =>
                {
                    b.Navigation("AuthTokens");

                    b.Navigation("WithdrawConfigs");
                });
#pragma warning restore 612, 618
        }
    }
}
