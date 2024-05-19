using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicrobotApi.Database.Migrations
{
    /// <inheritdoc />
    public partial class ScriptKey_paymentreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Keys",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Keys");
        }
    }
}
