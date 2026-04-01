using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDanhMucVietnamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DanhMucChaId",
                table: "DanhMucs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuongDan",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HienTrangChu",
                table: "DanhMucs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoTaSeo",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThuTu",
                table: "DanhMucs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TieuDeSeo",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrangThai",
                table: "DanhMucs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhMucChaId",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "DuongDan",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "HienTrangChu",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "MoTaSeo",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "ThuTu",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "TieuDeSeo",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "DanhMucs");
        }
    }
}
