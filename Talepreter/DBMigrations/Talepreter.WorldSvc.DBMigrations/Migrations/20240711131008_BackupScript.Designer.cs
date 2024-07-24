﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Talepreter.WorldSvc.DBContext;

#nullable disable

namespace Talepreter.WorldSvc.DBMigrations.Migrations
{
    [DbContext(typeof(WorldSvcDBContext))]
    [Migration("20240711131008_BackupScript")]
    partial class BackupScript
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence<int>("SubIndexSequence", "shared");

            modelBuilder.Entity("Talepreter.BaseTypes.Command", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ChapterId")
                        .HasColumnType("int");

                    b.Property<int>("PageId")
                        .HasColumnType("int");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<int>("Phase")
                        .HasColumnType("int");

                    b.Property<int>("SubIndex")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR shared.SubIndexSequence");

                    b.Property<string>("ArrayParameters")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Attempts")
                        .HasColumnType("int");

                    b.Property<string>("Comments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GrainId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GrainType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasChild")
                        .HasColumnType("bit");

                    b.Property<DateTime>("OperationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Parent")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Prequisite")
                        .HasColumnType("int");

                    b.Property<int>("Result")
                        .HasColumnType("int");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "ChapterId", "PageId", "Index", "Phase", "SubIndex");

                    b.HasIndex("TaleId", "TaleVersionId", "ChapterId", "PageId", "Phase");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("Talepreter.BaseTypes.ExtensionData", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("BaseId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<string>("PluginData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleId", "TaleVersionId", "Type");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.HasIndex("TaleId", "TaleVersionId", "BaseId", "Type", "PublishState");

                    b.ToTable("PluginRecords");
                });

            modelBuilder.Entity("Talepreter.BaseTypes.Trigger", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("GrainId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GrainType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Parameter")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TriggerAt")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "Type");

                    b.HasIndex("TaleId", "TaleVersionId", "State", "TriggerAt");

                    b.ToTable("Triggers");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Chapter", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<string>("Reference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleVersionId", "WorldName");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Page", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ChapterId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<long>("StartDate")
                        .HasColumnType("bigint");

                    b.Property<long>("StayAtLocation")
                        .HasColumnType("bigint");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleVersionId", "ChapterId", "Id");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Settlement", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("FirstVisited")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<long?>("LastVisited")
                        .HasColumnType("bigint");

                    b.Property<string>("PluginData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.ToTable("Settlements");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.World", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<string>("PluginData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.ToTable("Worlds");
                });

            modelBuilder.Entity("Talepreter.BaseTypes.Command", b =>
                {
                    b.OwnsMany("Talepreter.BaseTypes.NamedParameter", "NamedParameters", b1 =>
                        {
                            b1.Property<Guid>("CommandTaleId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("CommandTaleVersionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("CommandChapterId")
                                .HasColumnType("int");

                            b1.Property<int>("CommandPageId")
                                .HasColumnType("int");

                            b1.Property<int>("CommandIndex")
                                .HasColumnType("int");

                            b1.Property<int>("CommandPhase")
                                .HasColumnType("int");

                            b1.Property<int>("CommandSubIndex")
                                .HasColumnType("int");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<int>("Type")
                                .HasColumnType("int");

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("CommandTaleId", "CommandTaleVersionId", "CommandChapterId", "CommandPageId", "CommandIndex", "CommandPhase", "CommandSubIndex", "Id");

                            b1.ToTable("Commands");

                            b1.ToJson("NamedParameters");

                            b1.WithOwner()
                                .HasForeignKey("CommandTaleId", "CommandTaleVersionId", "CommandChapterId", "CommandPageId", "CommandIndex", "CommandPhase", "CommandSubIndex");
                        });

                    b.Navigation("NamedParameters");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Chapter", b =>
                {
                    b.HasOne("Talepreter.WorldSvc.DBContext.World", "World")
                        .WithMany("Chapters")
                        .HasForeignKey("TaleVersionId", "WorldName")
                        .HasPrincipalKey("TaleVersionId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("World");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Page", b =>
                {
                    b.HasOne("Talepreter.WorldSvc.DBContext.Chapter", "Owner")
                        .WithMany("Pages")
                        .HasForeignKey("TaleVersionId", "ChapterId")
                        .HasPrincipalKey("TaleVersionId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Talepreter.WorldSvc.DBContext.Journey", "Travel", b1 =>
                        {
                            b1.Property<Guid>("PageTaleId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("PageTaleVersionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("PageId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<long>("Duration")
                                .HasColumnType("bigint");

                            b1.HasKey("PageTaleId", "PageTaleVersionId", "PageId");

                            b1.ToTable("Pages");

                            b1.WithOwner()
                                .HasForeignKey("PageTaleId", "PageTaleVersionId", "PageId");

                            b1.OwnsOne("Talepreter.BaseTypes.Location", "Destination", b2 =>
                                {
                                    b2.Property<Guid>("JourneyPageTaleId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<Guid>("JourneyPageTaleVersionId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<string>("JourneyPageId")
                                        .HasColumnType("nvarchar(450)");

                                    b2.Property<string>("Extension")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("Settlement")
                                        .IsRequired()
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("JourneyPageTaleId", "JourneyPageTaleVersionId", "JourneyPageId");

                                    b2.ToTable("Pages");

                                    b2.WithOwner()
                                        .HasForeignKey("JourneyPageTaleId", "JourneyPageTaleVersionId", "JourneyPageId");
                                });

                            b1.Navigation("Destination")
                                .IsRequired();
                        });

                    b.OwnsOne("Talepreter.BaseTypes.Location", "Location", b1 =>
                        {
                            b1.Property<Guid>("PageTaleId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("PageTaleVersionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("PageId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("Extension")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Settlement")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("PageTaleId", "PageTaleVersionId", "PageId");

                            b1.ToTable("Pages");

                            b1.WithOwner()
                                .HasForeignKey("PageTaleId", "PageTaleVersionId", "PageId");
                        });

                    b.Navigation("Location")
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("Travel");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.Chapter", b =>
                {
                    b.Navigation("Pages");
                });

            modelBuilder.Entity("Talepreter.WorldSvc.DBContext.World", b =>
                {
                    b.Navigation("Chapters");
                });
#pragma warning restore 612, 618
        }
    }
}
