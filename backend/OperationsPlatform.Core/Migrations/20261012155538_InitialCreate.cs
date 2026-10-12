using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace OperationsPlatform.Core.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InputType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    A = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    B = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationLogs_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Operations",
                columns: new[] { "Id", "Definition", "Description", "InputType", "IsActive", "Key", "Kind", "Name" },
                values: new object[,]
                {
                    { 1, "a + b", null, "Number", true, "add", "Expression", "Addition" },
                    { 2, "a - b", null, "Number", true, "subtract", "Expression", "Subtraction" },
                    { 3, "a * b", null, "Number", true, "multiply", "Expression", "Multiplication" },
                    { 4, "a / b", null, "Number", true, "divide", "Expression", "Division" },
                    { 5, "Pow(a, b)", null, "Number", true, "power", "Expression", "Power" },
                    { 6, "Sqrt(a*a + b*b)", null, "Number", true, "hypotenuse", "Expression", "Hypotenuse (Pythagoras)" },
                    { 7, "concat", null, "Text", true, "concat", "Text", "String concatenation" },
                    { 8, "levenshtein", null, "Text", true, "levenshtein", "Text", "Edit distance between words" },
                    { 9, "anagram", null, "Text", true, "anagram", "Text", "Is it an anagram?" },
                    { 10, "{\"url\":\"https://api.open-meteo.com/v1/forecast?latitude={a}&longitude={b}&current=temperature_2m\",\"path\":\"current.temperature_2m\"}", null, "Coordinates", true, "weather", "Http", "Current temperature (latitude, longitude)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_OperationId_CreatedAtUtc",
                table: "OperationLogs",
                columns: new[] { "OperationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Key",
                table: "Operations",
                column: "Key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationLogs");

            migrationBuilder.DropTable(
                name: "Operations");
        }
    }
}
