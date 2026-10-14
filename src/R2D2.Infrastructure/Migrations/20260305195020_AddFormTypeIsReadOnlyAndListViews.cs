using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace R2D2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormTypeIsReadOnlyAndListViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadOnly",
                table: "FormFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FormType",
                table: "DynamicForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ListViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdateFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListViews_DynamicForms_CreateFormId",
                        column: x => x.CreateFormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ListViews_DynamicForms_UpdateFormId",
                        column: x => x.UpdateFormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListViews_DynamicLists_ListId",
                        column: x => x.ListId,
                        principalTable: "DynamicLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListViewColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldTechnicalName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    HeaderLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListViewColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListViewColumns_ListViews_ListViewId",
                        column: x => x.ListViewId,
                        principalTable: "ListViews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListViewColumns_ListViewId",
                table: "ListViewColumns",
                column: "ListViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ListViews_CreateFormId",
                table: "ListViews",
                column: "CreateFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ListViews_ListId",
                table: "ListViews",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListViews_UpdateFormId",
                table: "ListViews",
                column: "UpdateFormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListViewColumns");

            migrationBuilder.DropTable(
                name: "ListViews");

            migrationBuilder.DropColumn(
                name: "IsReadOnly",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "FormType",
                table: "DynamicForms");
        }
    }
}
