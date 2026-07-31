using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class TambahTabelLowonganFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lowongan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Judul = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalPosting = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lowongan", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lowongan");
        }
    }
}
