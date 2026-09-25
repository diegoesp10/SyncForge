using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Security.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "DisplayName", "Email", "EmailConfirmed", "EntraObjectId", "EntraTenantId", "IsActive", "LastSignedInAt", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("4c9868f3-10df-41ba-b33a-46ae2a021010"), 0, "syncforge-diego-pending-setup-v1", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DiegoEspina", "diegoespinarodriguez@gmail.com", false, null, null, true, null, true, null, "DIEGOESPINARODRIGUEZ@GMAIL.COM", "DIEGOESPINA", null, null, false, "syncforge-diego-pending-setup-v1", false, "DiegoEspina" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("4c9868f3-10df-41ba-b33a-46ae2a021003"), new Guid("4c9868f3-10df-41ba-b33a-46ae2a021010") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("4c9868f3-10df-41ba-b33a-46ae2a021003"), new Guid("4c9868f3-10df-41ba-b33a-46ae2a021010") });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4c9868f3-10df-41ba-b33a-46ae2a021010"));
        }
    }
}
