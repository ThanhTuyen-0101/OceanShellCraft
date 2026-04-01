using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBienTheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatLieu",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "KichThuoc",
                table: "SanPhams");

            migrationBuilder.CreateTable(
                name: "ChiTietBienThe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BienTheId = table.Column<int>(type: "int", nullable: false),
                    TenThuocTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietBienThe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietBienThe_BienTheSanPhams_BienTheId",
                        column: x => x.BienTheId,
                        principalTable: "BienTheSanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietBienThe_BienTheId",
                table: "ChiTietBienThe",
                column: "BienTheId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietBienThe");

            migrationBuilder.AddColumn<string>(
                name: "ChatLieu",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KichThuoc",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
