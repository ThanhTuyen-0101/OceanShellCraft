using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "SanPhams");
        }
    }
}
