using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talepreter.WorldSvc.DBMigrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateSequence<int>(
                name: "SubIndexSequence",
                schema: "shared");

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    SubIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR shared.SubIndexSequence"),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrainType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prequisite = table.Column<int>(type: "int", nullable: true),
                    HasChild = table.Column<bool>(type: "bit", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrayParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamedParameters = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => new { x.TaleId, x.TaleVersionId, x.ChapterId, x.PageId, x.Index, x.Phase, x.SubIndex });
                });

            migrationBuilder.CreateTable(
                name: "PluginRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PluginData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginRecords", x => new { x.TaleId, x.TaleVersionId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstVisited = table.Column<long>(type: "bigint", nullable: true),
                    LastVisited = table.Column<long>(type: "bigint", nullable: true),
                    PluginData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => new { x.TaleId, x.TaleVersionId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Triggers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    TriggerAt = table.Column<long>(type: "bigint", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrainType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrainId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Parameter = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triggers", x => new { x.TaleId, x.TaleVersionId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Worlds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PluginData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worlds", x => new { x.TaleId, x.TaleVersionId, x.Id });
                    table.UniqueConstraint("AK_Worlds_TaleVersionId_Id", x => new { x.TaleVersionId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorldName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => new { x.TaleId, x.TaleVersionId, x.Id });
                    table.UniqueConstraint("AK_Chapters_TaleVersionId_Id", x => new { x.TaleVersionId, x.Id });
                    table.ForeignKey(
                        name: "FK_Chapters_Worlds_TaleVersionId_WorldName",
                        columns: x => new { x.TaleVersionId, x.WorldName },
                        principalTable: "Worlds",
                        principalColumns: new[] { "TaleVersionId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Location_Settlement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location_Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<long>(type: "bigint", nullable: false),
                    StayAtLocation = table.Column<long>(type: "bigint", nullable: false),
                    Travel_Duration = table.Column<long>(type: "bigint", nullable: true),
                    Travel_Destination_Settlement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Travel_Destination_Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => new { x.TaleId, x.TaleVersionId, x.Id });
                    table.ForeignKey(
                        name: "FK_Pages_Chapters_TaleVersionId_ChapterId",
                        columns: x => new { x.TaleVersionId, x.ChapterId },
                        principalTable: "Chapters",
                        principalColumns: new[] { "TaleVersionId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TaleId",
                table: "Chapters",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TaleId_TaleVersionId",
                table: "Chapters",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TaleId_TaleVersionId_Id_PublishState",
                table: "Chapters",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TaleVersionId_WorldName",
                table: "Chapters",
                columns: new[] { "TaleVersionId", "WorldName" });

            migrationBuilder.CreateIndex(
                name: "IX_Commands_TaleId_TaleVersionId_ChapterId_PageId_Phase",
                table: "Commands",
                columns: new[] { "TaleId", "TaleVersionId", "ChapterId", "PageId", "Phase" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TaleId",
                table: "Pages",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TaleId_TaleVersionId",
                table: "Pages",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TaleId_TaleVersionId_Id_PublishState",
                table: "Pages",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TaleVersionId_ChapterId_Id",
                table: "Pages",
                columns: new[] { "TaleVersionId", "ChapterId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginRecords_TaleId",
                table: "PluginRecords",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginRecords_TaleId_TaleVersionId",
                table: "PluginRecords",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginRecords_TaleId_TaleVersionId_BaseId_Type_PublishState",
                table: "PluginRecords",
                columns: new[] { "TaleId", "TaleVersionId", "BaseId", "Type", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginRecords_TaleId_TaleVersionId_Id_PublishState",
                table: "PluginRecords",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginRecords_TaleId_TaleVersionId_Type",
                table: "PluginRecords",
                columns: new[] { "TaleId", "TaleVersionId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_TaleId",
                table: "Settlements",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_TaleId_TaleVersionId",
                table: "Settlements",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_TaleId_TaleVersionId_Id_PublishState",
                table: "Settlements",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TaleId",
                table: "Triggers",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TaleId_TaleVersionId_Id_Type",
                table: "Triggers",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TaleId_TaleVersionId_State_TriggerAt",
                table: "Triggers",
                columns: new[] { "TaleId", "TaleVersionId", "State", "TriggerAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_TaleId",
                table: "Worlds",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_TaleId_TaleVersionId",
                table: "Worlds",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_TaleId_TaleVersionId_Id_PublishState",
                table: "Worlds",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "PluginRecords");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "Triggers");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Worlds");

            migrationBuilder.DropSequence(
                name: "SubIndexSequence",
                schema: "shared");
        }
    }
}
