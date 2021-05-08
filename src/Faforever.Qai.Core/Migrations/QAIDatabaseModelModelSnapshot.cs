﻿// <auto-generated />
using System;
using Faforever.Qai.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Faforever.Qai.Core.Migrations
{
    [DbContext(typeof(QAIDatabaseModel))]
    partial class QAIDatabaseModelModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.4");

            modelBuilder.Entity("Faforever.Qai.Core.Structures.Configurations.DiscordGuildConfiguration", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FafLinks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Records")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegisteredRoles")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("RoleWhenLinked")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserBlacklist")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId");

                    b.ToTable("DiscordConfigs");
                });

            modelBuilder.Entity("Faforever.Qai.Core.Structures.Configurations.RelayConfiguration", b =>
                {
                    b.Property<ulong>("DiscordServer")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DiscordToIRCLinks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Webhooks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("DiscordServer");

                    b.ToTable("RelayConfigurations");
                });

            modelBuilder.Entity("Faforever.Qai.Core.Structures.Link.AccountLink", b =>
                {
                    b.Property<ulong>("DiscordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DiscordUsername")
                        .HasColumnType("TEXT");

                    b.Property<int>("FafId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FafUsername")
                        .HasColumnType("TEXT");

                    b.HasKey("DiscordId");

                    b.ToTable("AccountLinks");
                });
#pragma warning restore 612, 618
        }
    }
}
