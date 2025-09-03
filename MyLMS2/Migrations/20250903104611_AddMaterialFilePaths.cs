using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLMS2.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialFilePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoLink",
                table: "Materials",
                newName: "WordPath");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "Materials",
                newName: "VideoPath");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Materials",
                newName: "PptPath");

            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "WordPath",
                table: "Materials",
                newName: "VideoLink");

            migrationBuilder.RenameColumn(
                name: "VideoPath",
                table: "Materials",
                newName: "FileType");

            migrationBuilder.RenameColumn(
                name: "PptPath",
                table: "Materials",
                newName: "FilePath");
        }
    }
}
