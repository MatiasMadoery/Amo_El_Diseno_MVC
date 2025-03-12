using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmoElDiseno.Migrations
{
    /// <inheritdoc />
    public partial class Six : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Orders");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Orders",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
