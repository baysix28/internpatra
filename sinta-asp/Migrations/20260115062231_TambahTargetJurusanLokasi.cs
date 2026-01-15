using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class TambahTargetJurusanLokasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JudulPenelitian",
                table: "Pendaftarans",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetJurusan",
                table: "Pendaftarans",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetLokasi",
                table: "Pendaftarans",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetJurusan",
                table: "Pendaftarans");

            migrationBuilder.DropColumn(
                name: "TargetLokasi",
                table: "Pendaftarans");

            migrationBuilder.UpdateData(
                table: "Pendaftarans",
                keyColumn: "JudulPenelitian",
                keyValue: null,
                column: "JudulPenelitian",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "JudulPenelitian",
                table: "Pendaftarans",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
