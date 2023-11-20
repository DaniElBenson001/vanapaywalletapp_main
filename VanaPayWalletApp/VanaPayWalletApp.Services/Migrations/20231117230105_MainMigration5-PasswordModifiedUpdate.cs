using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanaPayWalletApp.Services.Migrations
{
    /// <inheritdoc />
    public partial class MainMigration5PasswordModifiedUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordModifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordModifiedAt",
                table: "Users");
        }
    }
}
