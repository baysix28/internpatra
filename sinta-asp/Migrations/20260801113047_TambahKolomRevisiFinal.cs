using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class TambahKolomRevisiFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatatanRevisi",
                table: "Pendaftarans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPendaftaran",
                table: "Pendaftarans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisiFields",
                table: "Pendaftarans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Pendaftarans",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatatanRevisi",
                table: "Pendaftarans");

            migrationBuilder.DropColumn(
                name: "NoPendaftaran",
                table: "Pendaftarans");

            migrationBuilder.DropColumn(
                name: "RevisiFields",
                table: "Pendaftarans");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Pendaftarans");
        }
    }
}
