using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocuNet.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedConnectionsWithInterfaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationInterface",
                table: "Connections",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceInterface",
                table: "Connections",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationInterface",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "SourceInterface",
                table: "Connections");
        }
    }
}
