using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationPropertiesAndRepositories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Courses_CourseId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_QuizAttempts_QuizAttemptId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Users_AwardedByUserId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Users_UserId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessionContents_Contents_ContentId",
                table: "StudySessionContents");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Courses_CourseId",
                table: "PointTransactions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_QuizAttempts_QuizAttemptId",
                table: "PointTransactions",
                column: "QuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "AttemptId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Users_AwardedByUserId",
                table: "PointTransactions",
                column: "AwardedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Users_UserId",
                table: "PointTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessionContents_Contents_ContentId",
                table: "StudySessionContents",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Courses_CourseId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_QuizAttempts_QuizAttemptId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Users_AwardedByUserId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Users_UserId",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessionContents_Contents_ContentId",
                table: "StudySessionContents");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Courses_CourseId",
                table: "PointTransactions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_QuizAttempts_QuizAttemptId",
                table: "PointTransactions",
                column: "QuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "AttemptId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Users_AwardedByUserId",
                table: "PointTransactions",
                column: "AwardedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Users_UserId",
                table: "PointTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessionContents_Contents_ContentId",
                table: "StudySessionContents",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId");
        }
    }
}
