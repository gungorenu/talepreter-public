using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talepreter.PersonSvc.DBMigrations.Migrations
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
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Physics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSeen = table.Column<long>(type: "bigint", nullable: true),
                    LastSeenLocation_Settlement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSeenLocation_Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PluginData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WriterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedChapter = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedPageInChapter = table.Column<int>(type: "int", nullable: false),
                    PublishState = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<long>(type: "bigint", nullable: true),
                    ExpiresAt = table.Column<long>(type: "bigint", nullable: true),
                    ExpiredAt = table.Column<long>(type: "bigint", nullable: true),
                    ExpireState = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => new { x.TaleId, x.TaleVersionId, x.Id });
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

            migrationBuilder.CreateIndex(
                name: "IX_Commands_TaleId_TaleVersionId_ChapterId_PageId_Phase",
                table: "Commands",
                columns: new[] { "TaleId", "TaleVersionId", "ChapterId", "PageId", "Phase" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TaleId",
                table: "Persons",
                column: "TaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TaleId_TaleVersionId",
                table: "Persons",
                columns: new[] { "TaleId", "TaleVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TaleId_TaleVersionId_Id_PublishState",
                table: "Persons",
                columns: new[] { "TaleId", "TaleVersionId", "Id", "PublishState" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TaleVersionId_ExpireState_ExpiresAt",
                table: "Persons",
                columns: new[] { "TaleVersionId", "ExpireState", "ExpiresAt" },
                filter: "[ExpiresAt] IS NOT NULL");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "PluginRecords");

            migrationBuilder.DropTable(
                name: "Triggers");

            migrationBuilder.DropSequence(
                name: "SubIndexSequence",
                schema: "shared");
        }
    }
}
