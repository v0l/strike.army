using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrikeArmy.Database.Migrations
{
    public partial class BoltCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BoltCardConfig_Counter",
                table: "WithdrawConfig",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_K0",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_K1",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_K2",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_K3",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoltCardConfig_K4",
                table: "WithdrawConfig",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoltCardConfig_Counter",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "BoltCardConfig_K0",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "BoltCardConfig_K1",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "BoltCardConfig_K2",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "BoltCardConfig_K3",
                table: "WithdrawConfig");

            migrationBuilder.DropColumn(
                name: "BoltCardConfig_K4",
                table: "WithdrawConfig");
        }
    }
}
