using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class UserNotification_CorrectFk2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserNotifications_Type_CreatedAt",
            //    table: "UserNotifications",
            //    columns: new[] { "Type", "CreatedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserNotifications_UserId_CreatedAt",
            //    table: "UserNotifications",
            //    columns: new[] { "UserId", "CreatedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserNotifications_UserId_IsRead",
            //    table: "UserNotifications",
            //    columns: new[] { "UserId", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_Priority_IsRead",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_Type_CreatedAt",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId_CreatedAt",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId_IsRead",
                table: "UserNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
