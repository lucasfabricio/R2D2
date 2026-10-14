using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace R2D2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToDynamicList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "DynamicLists",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "DynamicLists");
        }
    }
}
