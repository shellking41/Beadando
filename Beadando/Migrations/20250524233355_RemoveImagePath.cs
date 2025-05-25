using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beadando.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Questions",
                type: "TEXT",
                nullable: true);
        }
    }
}
