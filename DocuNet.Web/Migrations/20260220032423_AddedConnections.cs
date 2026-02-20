using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocuNet.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DestinationDeviceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Speed = table.Column<string>(type: "TEXT", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_Devices_DestinationDeviceId",
                        column: x => x.DestinationDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Connections_Devices_SourceDeviceId",
                        column: x => x.SourceDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Connections_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_DestinationDeviceId",
                table: "Connections",
                column: "DestinationDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_OrganizationId",
                table: "Connections",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_SourceDeviceId",
                table: "Connections",
                column: "SourceDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
