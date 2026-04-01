using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangDanhGiaBaiViet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemDanhGiaTrungBinh",
                table: "BaiViets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LuotThich",
                table: "BaiViets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LuotXem",
                table: "BaiViets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDang",
                table: "BaiViets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TacGia",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TheTags",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TongLuotDanhGia",
                table: "BaiViets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlSlug",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DanhGiaBaiViets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    BaiVietId = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaDuyet = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaBaiViets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaBaiViets_BaiViets_BaiVietId",
                        column: x => x.BaiVietId,
                        principalTable: "BaiViets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaBaiViets_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaBaiViets_BaiVietId",
                table: "DanhGiaBaiViets",
                column: "BaiVietId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaBaiViets_NguoiDungId",
                table: "DanhGiaBaiViets",
                column: "NguoiDungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGiaBaiViets");

            migrationBuilder.DropColumn(
                name: "DiemDanhGiaTrungBinh",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "LuotThich",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "LuotXem",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "NgayDang",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TacGia",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TheTags",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TongLuotDanhGia",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "UrlSlug",
                table: "BaiViets");
        }
    }
}
