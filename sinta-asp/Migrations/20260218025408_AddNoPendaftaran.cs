using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class AddNoPendaftaran : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoPendaftaran",
                table: "pendaftaran_magang",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoPendaftaran",
                table: "pendaftaran_magang");
        }
    }
}
