using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceanShellCraft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "HoTen",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "DonHangs");

            migrationBuilder.RenameColumn(
                name: "GiaBan",
                table: "ChiTietDonHangs",
                newName: "GiaLucMua");

            migrationBuilder.AddColumn<int>(
                name: "NguoiDungId",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_NguoiDungId",
                table: "DonHangs",
                column: "NguoiDungId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_NguoiDungs_NguoiDungId",
                table: "DonHangs",
                column: "NguoiDungId",
                principalTable: "NguoiDungs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_NguoiDungs_NguoiDungId",
                table: "DonHangs");

            migrationBuilder.DropIndex(
                name: "IX_DonHangs_NguoiDungId",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "DonHangs");

            migrationBuilder.RenameColumn(
                name: "GiaLucMua",
                table: "ChiTietDonHangs",
                newName: "GiaBan");

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HoTen",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
