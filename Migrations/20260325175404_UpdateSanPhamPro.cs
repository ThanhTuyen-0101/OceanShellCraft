using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSanPhamPro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoLuongDaBan",
                table: "SanPhams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "SanPhams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BienTheSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    TenBienThe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaRieng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongRieng = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienTheSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienTheSanPhams_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuTonKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NgayThayDoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongThayDoi = table.Column<int>(type: "int", nullable: false),
                    LoaiThayDoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTonKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuTonKhos_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienTheSanPhams_SanPhamId",
                table: "BienTheSanPhams",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTonKhos_SanPhamId",
                table: "LichSuTonKhos",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BienTheSanPhams");

            migrationBuilder.DropTable(
                name: "LichSuTonKhos");

            migrationBuilder.DropColumn(
                name: "SoLuongDaBan",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "SanPhams");
        }
    }
}
