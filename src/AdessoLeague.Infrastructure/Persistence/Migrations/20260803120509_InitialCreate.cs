using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdessoLeague.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "countries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_countries", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "draws",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                drawn_by_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                drawn_by_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                group_count = table.Column<int>(type: "integer", nullable: false),
                seed = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draws", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "teams",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                country_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_teams", x => x.id);
                table.ForeignKey(
                    name: "fk_teams_countries_country_id",
                    column: x => x.country_id,
                    principalTable: "countries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "draw_groups",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                ordinal = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_groups", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_groups_draws_draw_id",
                    column: x => x.draw_id,
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_group_teams",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                team_id = table.Column<Guid>(type: "uuid", nullable: false),
                position = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_group_teams", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_group_teams_draw_groups_draw_group_id",
                    column: x => x.draw_group_id,
                    principalTable: "draw_groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_draw_group_teams_draws_draw_id",
                    column: x => x.draw_id,
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_draw_group_teams_teams_team_id",
                    column: x => x.team_id,
                    principalTable: "teams",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "countries",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0000-000000000001"), "Türkiye" },
                { new Guid("00000000-0000-0000-0000-000000000002"), "Almanya" },
                { new Guid("00000000-0000-0000-0000-000000000003"), "Fransa" },
                { new Guid("00000000-0000-0000-0000-000000000004"), "Hollanda" },
                { new Guid("00000000-0000-0000-0000-000000000005"), "Portekiz" },
                { new Guid("00000000-0000-0000-0000-000000000006"), "İtalya" },
                { new Guid("00000000-0000-0000-0000-000000000007"), "İspanya" },
                { new Guid("00000000-0000-0000-0000-000000000008"), "Belçika" }
            });

        migrationBuilder.InsertData(
            table: "teams",
            columns: new[] { "id", "country_id", "name" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), "Adesso İstanbul" },
                { new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), "Adesso Ankara" },
                { new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), "Adesso İzmir" },
                { new Guid("00000000-0000-0000-0001-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), "Adesso Antalya" },
                { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0000-000000000002"), "Adesso Berlin" },
                { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0000-000000000002"), "Adesso Frankfurt" },
                { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0000-000000000002"), "Adesso Münih" },
                { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0000-000000000002"), "Adesso Dortmund" },
                { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000003"), "Adesso Paris" },
                { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0000-000000000003"), "Adesso Marsilya" },
                { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0000-000000000003"), "Adesso Nice" },
                { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0000-000000000003"), "Adesso Lyon" },
                { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000004"), "Adesso Amsterdam" },
                { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0000-000000000004"), "Adesso Rotterdam" },
                { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0000-000000000004"), "Adesso Lahey" },
                { new Guid("00000000-0000-0000-0004-000000000004"), new Guid("00000000-0000-0000-0000-000000000004"), "Adesso Eindhoven" },
                { new Guid("00000000-0000-0000-0005-000000000001"), new Guid("00000000-0000-0000-0000-000000000005"), "Adesso Lisbon" },
                { new Guid("00000000-0000-0000-0005-000000000002"), new Guid("00000000-0000-0000-0000-000000000005"), "Adesso Porto" },
                { new Guid("00000000-0000-0000-0005-000000000003"), new Guid("00000000-0000-0000-0000-000000000005"), "Adesso Braga" },
                { new Guid("00000000-0000-0000-0005-000000000004"), new Guid("00000000-0000-0000-0000-000000000005"), "Adesso Coimbra" },
                { new Guid("00000000-0000-0000-0006-000000000001"), new Guid("00000000-0000-0000-0000-000000000006"), "Adesso Roma" },
                { new Guid("00000000-0000-0000-0006-000000000002"), new Guid("00000000-0000-0000-0000-000000000006"), "Adesso Milano" },
                { new Guid("00000000-0000-0000-0006-000000000003"), new Guid("00000000-0000-0000-0000-000000000006"), "Adesso Venedik" },
                { new Guid("00000000-0000-0000-0006-000000000004"), new Guid("00000000-0000-0000-0000-000000000006"), "Adesso Napoli" },
                { new Guid("00000000-0000-0000-0007-000000000001"), new Guid("00000000-0000-0000-0000-000000000007"), "Adesso Sevilla" },
                { new Guid("00000000-0000-0000-0007-000000000002"), new Guid("00000000-0000-0000-0000-000000000007"), "Adesso Madrid" },
                { new Guid("00000000-0000-0000-0007-000000000003"), new Guid("00000000-0000-0000-0000-000000000007"), "Adesso Barselona" },
                { new Guid("00000000-0000-0000-0007-000000000004"), new Guid("00000000-0000-0000-0000-000000000007"), "Adesso Granada" },
                { new Guid("00000000-0000-0000-0008-000000000001"), new Guid("00000000-0000-0000-0000-000000000008"), "Adesso Brüksel" },
                { new Guid("00000000-0000-0000-0008-000000000002"), new Guid("00000000-0000-0000-0000-000000000008"), "Adesso Brugge" },
                { new Guid("00000000-0000-0000-0008-000000000003"), new Guid("00000000-0000-0000-0000-000000000008"), "Adesso Gent" },
                { new Guid("00000000-0000-0000-0008-000000000004"), new Guid("00000000-0000-0000-0000-000000000008"), "Adesso Anvers" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_countries_name",
            table: "countries",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_group_teams_draw_group_id_team_id",
            table: "draw_group_teams",
            columns: new[] { "draw_group_id", "team_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_group_teams_draw_id_team_id",
            table: "draw_group_teams",
            columns: new[] { "draw_id", "team_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_group_teams_team_id",
            table: "draw_group_teams",
            column: "team_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_groups_draw_id_name",
            table: "draw_groups",
            columns: new[] { "draw_id", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_teams_country_id",
            table: "teams",
            column: "country_id");

        migrationBuilder.CreateIndex(
            name: "ix_teams_name",
            table: "teams",
            column: "name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "draw_group_teams");

        migrationBuilder.DropTable(
            name: "draw_groups");

        migrationBuilder.DropTable(
            name: "teams");

        migrationBuilder.DropTable(
            name: "draws");

        migrationBuilder.DropTable(
            name: "countries");
    }
}
