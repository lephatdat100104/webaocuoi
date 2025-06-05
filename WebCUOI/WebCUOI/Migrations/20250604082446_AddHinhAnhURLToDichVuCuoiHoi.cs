using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCUOI.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhURLToDichVuCuoiHoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnhURL",
                table: "DichVuCuoiHoi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnhURL",
                table: "DichVuCuoiHoi");
        }
    }
}
