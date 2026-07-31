using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sinta_asp.Migrations
{
    /// <inheritdoc />
    public partial class FixKolomLowonganSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Lowongan', 'Company') IS NULL ALTER TABLE Lowongan ADD Company NVARCHAR(MAX) NULL;
                IF COL_LENGTH('Lowongan', 'CreatedAt') IS NULL ALTER TABLE Lowongan ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                IF COL_LENGTH('Lowongan', 'Description') IS NULL ALTER TABLE Lowongan ADD Description NVARCHAR(MAX) NULL;
                IF COL_LENGTH('Lowongan', 'ImageUrl') IS NULL ALTER TABLE Lowongan ADD ImageUrl NVARCHAR(MAX) NULL;
                IF COL_LENGTH('Lowongan', 'Region') IS NULL ALTER TABLE Lowongan ADD Region NVARCHAR(MAX) NULL;
                IF COL_LENGTH('Lowongan', 'Title') IS NULL ALTER TABLE Lowongan ADD Title NVARCHAR(MAX) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}