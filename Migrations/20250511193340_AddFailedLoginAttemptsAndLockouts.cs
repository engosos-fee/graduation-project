using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_graduation.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedLoginAttemptsAndLockouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastConfirmationEmailSent",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastConfirmationEmailSent",
                table: "Users");
        }
    }
}
