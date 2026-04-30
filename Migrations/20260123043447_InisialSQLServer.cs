using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class InisialSQLServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pendaftarans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoHp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TglLahir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathFoto3x4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Universitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fakultas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jurusan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LokasiPenelitian = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JudulPenelitian = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TglMulai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TglSelesai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetLokasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetJurusan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathCV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathProposal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathSurat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pendaftarans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pendaftarans");
        }
    }
}
