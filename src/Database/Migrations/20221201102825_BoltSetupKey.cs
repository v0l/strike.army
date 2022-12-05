using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrikeArmy.Database.Migrations
{
    public partial class BoltSetupKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_SetupKey",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoltCardConfig_SetupKey",
                table: "WithdrawConfig");
        }
    }
}
