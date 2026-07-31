using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mahasiswa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipePendaftaran = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoHP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKampus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fakultas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jurusan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NIM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetailTambahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CVPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProposalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuratKampusPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mahasiswa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pendaftaran_magang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FotoProfil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaLengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailPribadi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempatLahir = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoHp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaPerguruanTinggi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fakultas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Jurusan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NIM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lokasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RekomendasiPegawai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MulaiMagang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelesaiMagang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileCv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSuratPengantar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileProposal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pendaftaran_magang", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Mahasiswa");

            migrationBuilder.DropTable(
                name: "pendaftaran_magang");

            migrationBuilder.DropTable(
                name: "Pendaftarans");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
