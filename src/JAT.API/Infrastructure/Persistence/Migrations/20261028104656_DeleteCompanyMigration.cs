using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JAT.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCompanyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Companies_CompanyId",
                schema: "jat",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Companies_CompanyId",
                schema: "jat",
                table: "JobApplications");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "jat");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CompanyId",
                schema: "jat",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                schema: "jat",
                table: "JobApplications",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                schema: "jat",
                table: "Contacts",
                newName: "JobApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_CompanyId",
                schema: "jat",
                table: "Contacts",
                newName: "IX_Contacts_JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_JobApplications_JobApplicationId",
                schema: "jat",
                table: "Contacts",
                column: "JobApplicationId",
                principalSchema: "jat",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_JobApplications_JobApplicationId",
                schema: "jat",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                schema: "jat",
                table: "JobApplications",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "JobApplicationId",
                schema: "jat",
                table: "Contacts",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_JobApplicationId",
                schema: "jat",
                table: "Contacts",
                newName: "IX_Contacts_CompanyId");

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "jat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CompanyId",
                schema: "jat",
                table: "JobApplications",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Companies_CompanyId",
                schema: "jat",
                table: "Contacts",
                column: "CompanyId",
                principalSchema: "jat",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Companies_CompanyId",
                schema: "jat",
                table: "JobApplications",
                column: "CompanyId",
                principalSchema: "jat",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
