﻿// <auto-generated />
using System;
using ClinicalEpilepsyApp.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    [DbContext(typeof(ClinicalEpilepsyAppDbContext))]
    [Migration("20240502082419_ProcessedMeasurementToListOfInts")]
    partial class ProcessedMeasurementToListOfInts
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ClinicalEpilepsyApp.Domain.DBModels.EcgAlarm", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("AlarmTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<int>("CSI100")
                        .HasColumnType("int");

                    b.Property<int>("CSI30")
                        .HasColumnType("int");

                    b.Property<int>("CSI50")
                        .HasColumnType("int");

                    b.Property<Guid>("EcgProcessedMeasurementId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ModCSI100")
                        .HasColumnType("int");

                    b.Property<int>("ModCSI30")
                        .HasColumnType("int");

                    b.Property<int>("ModCSI50")
                        .HasColumnType("int");

                    b.Property<int>("PatientCSIThreshold")
                        .HasColumnType("int");

                    b.Property<string>("PatientId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PatientModCSIThreshold")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EcgAlarms");
                });

            modelBuilder.Entity("ClinicalEpilepsyApp.Domain.DBModels.EcgProcessedMeasurement", b =>
                {
                    b.Property<Guid>("ProcessedMeasurementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PatientId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProcessedEcgChannel1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProcessedEcgChannel2")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProcessedEcgChannel3")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("ProcessedMeasurementId");

                    b.ToTable("EcgProcessedMeasurements");
                });

            modelBuilder.Entity("ClinicalEpilepsyApp.Domain.DBModels.Patient", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CSIThreshold")
                        .HasColumnType("int");

                    b.Property<int>("ModCSIThreshold")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Patients");
                });
#pragma warning restore 612, 618
        }
    }
}
