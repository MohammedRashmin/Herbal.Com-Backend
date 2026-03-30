using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Com.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryPublicIdToProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "ProductImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "ProductImages");
        }
    }
}
