using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchDropsDiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowFkConstraintColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drops_drop_owners_drop_owner_id",
                table: "drops");

            migrationBuilder.DropForeignKey(
                name: "fk_drops_games_game_id",
                table: "drops");

            migrationBuilder.DropForeignKey(
                name: "fk_time_based_drops_drops_parent_drop_id",
                table: "time_based_drops");

            migrationBuilder.DropIndex(
                name: "ix_time_based_drops_parent_drop_id",
                table: "time_based_drops");

            migrationBuilder.DropIndex(
                name: "ix_drops_drop_owner_id",
                table: "drops");

            migrationBuilder.DropIndex(
                name: "ix_drops_game_id",
                table: "drops");

            migrationBuilder.DropColumn(
                name: "parent_drop_id1",
                table: "time_based_drops");

            migrationBuilder.DropColumn(
                name: "drop_owner_id1",
                table: "drops");

            migrationBuilder.DropColumn(
                name: "game_id1",
                table: "drops");

            migrationBuilder.CreateIndex(
                name: "ix_time_based_drops_parent_drop_id",
                table: "time_based_drops",
                column: "parent_drop_id");

            migrationBuilder.CreateIndex(
                name: "ix_drops_drop_owner_id",
                table: "drops",
                column: "drop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_drops_game_id",
                table: "drops",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "fk_drops_drop_owners_drop_owner_id",
                table: "drops",
                column: "drop_owner_id",
                principalTable: "drop_owners",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_drops_games_game_id",
                table: "drops",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_time_based_drops_drops_parent_drop_id",
                table: "time_based_drops",
                column: "parent_drop_id",
                principalTable: "drops",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drops_drop_owners_drop_owner_id",
                table: "drops");

            migrationBuilder.DropForeignKey(
                name: "fk_drops_games_game_id",
                table: "drops");

            migrationBuilder.DropForeignKey(
                name: "fk_time_based_drops_drops_parent_drop_id",
                table: "time_based_drops");

            migrationBuilder.DropIndex(
                name: "ix_time_based_drops_parent_drop_id",
                table: "time_based_drops");

            migrationBuilder.DropIndex(
                name: "ix_drops_drop_owner_id",
                table: "drops");

            migrationBuilder.DropIndex(
                name: "ix_drops_game_id",
                table: "drops");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_drop_id1",
                table: "time_based_drops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<short>(
                name: "drop_owner_id1",
                table: "drops",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "game_id1",
                table: "drops",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "ix_time_based_drops_parent_drop_id",
                table: "time_based_drops",
                column: "parent_drop_id1");

            migrationBuilder.CreateIndex(
                name: "ix_drops_drop_owner_id",
                table: "drops",
                column: "drop_owner_id1");

            migrationBuilder.CreateIndex(
                name: "ix_drops_game_id",
                table: "drops",
                column: "game_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_drops_drop_owners_drop_owner_id",
                table: "drops",
                column: "drop_owner_id1",
                principalTable: "drop_owners",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_drops_games_game_id",
                table: "drops",
                column: "game_id1",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_time_based_drops_drops_parent_drop_id",
                table: "time_based_drops",
                column: "parent_drop_id1",
                principalTable: "drops",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
