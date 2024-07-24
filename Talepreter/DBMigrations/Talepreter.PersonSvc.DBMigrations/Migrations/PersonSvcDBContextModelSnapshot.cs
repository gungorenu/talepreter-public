﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Talepreter.PersonSvc.DBContext;

#nullable disable

namespace Talepreter.PersonSvc.DBMigrations.Migrations
{
    [DbContext(typeof(PersonSvcDBContext))]
    partial class PersonSvcDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("Talepreter.PersonSvc.DBContext.Person", b =>
                {
                    b.Property<Guid>("TaleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TaleVersionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ExpireState")
                        .HasColumnType("int");

                    b.Property<long?>("ExpiredAt")
                        .HasColumnType("bigint");

                    b.Property<long?>("ExpiresAt")
                        .HasColumnType("bigint");

                    b.Property<string>("Identity")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("LastSeen")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastUpdatedChapter")
                        .HasColumnType("int");

                    b.Property<int>("LastUpdatedPageInChapter")
                        .HasColumnType("int");

                    b.Property<string>("Physics")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PluginData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PublishState")
                        .HasColumnType("int");

                    b.Property<long?>("StartsAt")
                        .HasColumnType("bigint");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaleId", "TaleVersionId", "Id");

                    b.HasIndex("TaleId");

                    b.HasIndex("TaleId", "TaleVersionId");

                    b.HasIndex("TaleVersionId", "ExpireState", "ExpiresAt")
                        .HasFilter("[ExpiresAt] IS NOT NULL");

                    b.HasIndex("TaleId", "TaleVersionId", "Id", "PublishState");

                    b.ToTable("Persons");
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

            modelBuilder.Entity("Talepreter.PersonSvc.DBContext.Person", b =>
                {
                    b.OwnsOne("Talepreter.BaseTypes.Location", "LastSeenLocation", b1 =>
                        {
                            b1.Property<Guid>("PersonTaleId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("PersonTaleVersionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("PersonId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("Extension")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Settlement")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("PersonTaleId", "PersonTaleVersionId", "PersonId");

                            b1.ToTable("Persons");

                            b1.WithOwner()
                                .HasForeignKey("PersonTaleId", "PersonTaleVersionId", "PersonId");
                        });

                    b.OwnsOne("Talepreter.PersonSvc.DBContext.NotesMetadata", "Notes", b1 =>
                        {
                            b1.Property<Guid>("PersonTaleId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("PersonTaleVersionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("PersonId")
                                .HasColumnType("nvarchar(450)");

                            b1.HasKey("PersonTaleId", "PersonTaleVersionId", "PersonId");

                            b1.ToTable("Persons");

                            b1.ToJson("Notes");

                            b1.WithOwner()
                                .HasForeignKey("PersonTaleId", "PersonTaleVersionId", "PersonId");

                            b1.OwnsMany("Talepreter.PersonSvc.DBContext.NoteEntry", "List", b2 =>
                                {
                                    b2.Property<Guid>("NotesMetadataPersonTaleId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<Guid>("NotesMetadataPersonTaleVersionId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<string>("NotesMetadataPersonId")
                                        .HasColumnType("nvarchar(450)");

                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("int");

                                    b2.Property<int>("Chapter")
                                        .HasColumnType("int");

                                    b2.Property<string>("Notes")
                                        .IsRequired()
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<int>("Page")
                                        .HasColumnType("int");

                                    b2.HasKey("NotesMetadataPersonTaleId", "NotesMetadataPersonTaleVersionId", "NotesMetadataPersonId", "Id");

                                    b2.ToTable("Persons");

                                    b2.WithOwner()
                                        .HasForeignKey("NotesMetadataPersonTaleId", "NotesMetadataPersonTaleVersionId", "NotesMetadataPersonId");
                                });

                            b1.Navigation("List");
                        });

                    b.Navigation("LastSeenLocation");

                    b.Navigation("Notes");
                });
#pragma warning restore 612, 618
        }
    }
}
