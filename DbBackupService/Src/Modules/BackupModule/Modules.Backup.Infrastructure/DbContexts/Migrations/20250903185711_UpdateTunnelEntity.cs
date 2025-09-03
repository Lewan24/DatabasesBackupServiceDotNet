using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Backup.Infrastructure.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTunnelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "DbServerTunnels",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DbServerTunnels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DbServerTunnels",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LocalPort",
                table: "DbServerTunnels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrivateKeyContent",
                table: "DbServerTunnels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteHost",
                table: "DbServerTunnels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RemotePort",
                table: "DbServerTunnels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SshPort",
                table: "DbServerTunnels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UsePasswordAuth",
                table: "DbServerTunnels",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "LocalPort",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "PrivateKeyContent",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "RemoteHost",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "RemotePort",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "SshPort",
                table: "DbServerTunnels");

            migrationBuilder.DropColumn(
                name: "UsePasswordAuth",
                table: "DbServerTunnels");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "DbServerTunnels",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
