using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class FixUserAchievementCourseFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Courses_CourseId1",
                table: "UserAchievements");

            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_CourseId1",
                table: "UserAchievements");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "UserAchievements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "UserAchievements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_CourseId1",
                table: "UserAchievements",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Courses_CourseId1",
                table: "UserAchievements",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}
