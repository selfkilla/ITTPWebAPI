using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ITTPWebAPI.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Guid",
                keyValue: new Guid("20ee7f6f-e2ca-45d4-804d-49a5b9ff4df4"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Guid", "Admin", "Birthday", "CreatedBy", "CreatedOn", "Gender", "Login", "ModifiedBy", "ModifiedOn", "Name", "Password", "RevokedBy", "RevokedOn" },
                values: new object[] { new Guid("0583fe2b-be7e-4bef-9504-da2effe1a94a"), true, null, "Admin", new DateTime(2022, 5, 22, 20, 49, 0, 757, DateTimeKind.Local).AddTicks(916), 2, "Admin", "Admin", new DateTime(2022, 5, 22, 20, 49, 0, 758, DateTimeKind.Local).AddTicks(3510), "Danabek", "Admin123", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Guid",
                keyValue: new Guid("0583fe2b-be7e-4bef-9504-da2effe1a94a"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Guid", "Admin", "Birthday", "CreatedBy", "CreatedOn", "Gender", "Login", "ModifiedBy", "ModifiedOn", "Name", "Password", "RevokedBy", "RevokedOn" },
                values: new object[] { new Guid("20ee7f6f-e2ca-45d4-804d-49a5b9ff4df4"), true, null, "Admin", new DateTime(2022, 5, 22, 20, 2, 32, 344, DateTimeKind.Local).AddTicks(7290), 2, "Admin", "Admin", new DateTime(2022, 5, 22, 20, 2, 32, 345, DateTimeKind.Local).AddTicks(8736), "Danabek", "Admin123", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
