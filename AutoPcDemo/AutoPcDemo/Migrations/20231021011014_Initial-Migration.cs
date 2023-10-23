using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPcDemoWebsite.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestDatas",
                columns: table => new
                {
                    ID = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryOne = table.Column<string>(type: "TEXT", nullable: true),
                    CategoryTwo = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDatas", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestDatas");
        }
    }
}
