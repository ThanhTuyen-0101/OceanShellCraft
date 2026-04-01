using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatBangGiamGiaMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "BaiViets");

            migrationBuilder.AlterColumn<string>(
                name: "TenVoucher",
                table: "GiamGias",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaVoucher",
                table: "GiamGias",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<double>(
                name: "DonHangToiThieu",
                table: "GiamGias",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GiaTriGiam",
                table: "GiamGias",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GiamToiDa",
                table: "GiamGias",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GioiHanMoiKhach",
                table: "GiamGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsKichHoat",
                table: "GiamGias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoaiGiam",
                table: "GiamGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "GiamGias",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "GiamGias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SoLuongDaDung",
                table: "GiamGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongGioiHan",
                table: "GiamGias",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonHangToiThieu",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "GiaTriGiam",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "GiamToiDa",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "GioiHanMoiKhach",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "IsKichHoat",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "LoaiGiam",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "SoLuongDaDung",
                table: "GiamGias");

            migrationBuilder.DropColumn(
                name: "SoLuongGioiHan",
                table: "GiamGias");

            migrationBuilder.AlterColumn<string>(
                name: "TenVoucher",
                table: "GiamGias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "MaVoucher",
                table: "GiamGias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
