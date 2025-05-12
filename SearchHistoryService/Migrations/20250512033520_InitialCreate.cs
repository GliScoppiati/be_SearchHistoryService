using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchHistoryService.SearchHistoryService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilterType = table.Column<string>(type: "text", nullable: false),
                    FilterName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchFilters_SearchHistories_SearchHistoryId",
                        column: x => x.SearchHistoryId,
                        principalTable: "SearchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchFilters_SearchHistoryId",
                table: "SearchFilters",
                column: "SearchHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchFilters");

            migrationBuilder.DropTable(
                name: "SearchHistories");
        }
    }
}
