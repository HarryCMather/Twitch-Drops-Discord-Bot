using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TwitchDropsDiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drop_owners",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drop_owners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    should_alert = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<short>(type: "smallint", nullable: false),
                    drop_owner_id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    account_link_url = table.Column<string>(type: "text", nullable: true),
                    details_url = table.Column<string>(type: "text", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    drop_owner_id1 = table.Column<short>(type: "smallint", nullable: false),
                    game_id1 = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drops", x => x.id);
                    table.ForeignKey(
                        name: "fk_drops_drop_owners_drop_owner_id",
                        column: x => x.drop_owner_id1,
                        principalTable: "drop_owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_drops_games_game_id",
                        column: x => x.game_id1,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "time_based_drops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_drop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    required_minutes_watched = table.Column<short>(type: "smallint", nullable: false),
                    alerted_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    parent_drop_id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_based_drops", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_based_drops_drops_parent_drop_id",
                        column: x => x.parent_drop_id1,
                        principalTable: "drops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_drop_owners_name",
                table: "drop_owners",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_drops_drop_owner_id",
                table: "drops",
                column: "drop_owner_id1");

            migrationBuilder.CreateIndex(
                name: "ix_drops_game_id",
                table: "drops",
                column: "game_id1");

            migrationBuilder.CreateIndex(
                name: "ix_games_name",
                table: "games",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_time_based_drops_parent_drop_id",
                table: "time_based_drops",
                column: "parent_drop_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_based_drops");

            migrationBuilder.DropTable(
                name: "drops");

            migrationBuilder.DropTable(
                name: "drop_owners");

            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
