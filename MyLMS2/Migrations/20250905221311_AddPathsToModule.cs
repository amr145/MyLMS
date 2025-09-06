

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLMS2.Migrations
{
    /// <inheritdoc />
    public partial class AddPathsToModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PptPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "PptPath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "VideoPath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "WordPath",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
