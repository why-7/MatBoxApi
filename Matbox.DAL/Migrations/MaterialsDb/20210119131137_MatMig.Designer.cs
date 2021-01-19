﻿// <auto-generated />
using System;
using Matbox.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Matbox.DAL.Migrations.MaterialsDb
{
    [DbContext(typeof(MaterialsDbContext))]
    [Migration("20210119131137_MatMig")]
    partial class MatMig
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Matbox.DAL.Models.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("MaterialName")
                        .HasColumnType("text");

                    b.Property<DateTime>("MetaDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<double>("MetaFileSize")
                        .HasColumnType("double precision");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<int>("VersionNumber")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Materials");
                });
#pragma warning restore 612, 618
        }
    }
}