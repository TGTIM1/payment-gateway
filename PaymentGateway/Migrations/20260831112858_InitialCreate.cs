using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusHistory",
                table: "StatusHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusHistory",
                table: "StatusHistory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_PaymentId",
                table: "StatusHistory",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusHistory",
                table: "StatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_StatusHistory_PaymentId",
                table: "StatusHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusHistory",
                table: "StatusHistory",
                column: "PaymentId");
        }
    }
}
