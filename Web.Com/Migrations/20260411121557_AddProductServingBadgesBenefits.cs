using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Com.Migrations
{
    /// <inheritdoc />
    public partial class AddProductServingBadgesBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Badges",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServingSize",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServingsPerContainer",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Badges",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ServingSize",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ServingsPerContainer",
                table: "Products");
        }
    }
}
