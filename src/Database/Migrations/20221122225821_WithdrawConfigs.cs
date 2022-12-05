using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrikeArmy.Database.Migrations
{
    public partial class WithdrawConfigs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigReusable_Interval",
                table: "WithdrawConfig",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConfigReusable_Limit",
                table: "WithdrawConfig",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WithdrawConfig",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "WithdrawConfig",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WithdrawConfigPayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WithdrawConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrikeQuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoutingFee = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    PayeeNodePubKey = table.Column<string>(type: "text", nullable: false),
                    Pr = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawConfigPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawConfigPayment_WithdrawConfig_WithdrawConfigId",
                        column: x => x.WithdrawConfigId,
                        principalTable: "WithdrawConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawConfigPayment_Created_Status",
                table: "WithdrawConfigPayment",
                columns: new[] { "Created", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawConfigPayment_WithdrawConfigId",
                table: "WithdrawConfigPayment",
                column: "WithdrawConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WithdrawConfigPayment");

            migrationBuilder.DropColumn(
                name: "ConfigReusable_Interval",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "ConfigReusable_Limit",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "WithdrawConfig");
        }
    }
}
