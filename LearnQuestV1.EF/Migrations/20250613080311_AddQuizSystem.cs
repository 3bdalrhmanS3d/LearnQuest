using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Contents_ContentId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_InstructorId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Contents_ContentId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Levels_LevelId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Sections_SectionId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_InstructorId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_SelectedOptionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAnswer_AnswerType",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ContentId",
                table: "Quizzes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quiz_HierarchyConstraint",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_QuizId",
                table: "QuizQuestions");

            migrationBuilder.AlterColumn<int>(
                name: "PassingScore",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 70,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "HasCode",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "OptionText",
                table: "QuestionOptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_AttemptId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ContentId_SectionId_LevelId",
                table: "Quizzes",
                columns: new[] { "ContentId", "SectionId", "LevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizType",
                table: "Quizzes",
                column: "QuizType");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId_OrderIndex",
                table: "QuizQuestions",
                columns: new[] { "QuizId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId_QuestionId",
                table: "QuizQuestions",
                columns: new[] { "QuizId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_UserId_AttemptNumber",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "UserId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionType",
                table: "Questions",
                column: "QuestionType");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_OrderIndex",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "OrderIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Contents_ContentId",
                table: "Questions",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_InstructorId",
                table: "Questions",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Contents_ContentId",
                table: "Quizzes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Levels_LevelId",
                table: "Quizzes",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Sections_SectionId",
                table: "Quizzes",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_InstructorId",
                table: "Quizzes",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_SelectedOptionId",
                table: "UserAnswers",
                column: "SelectedOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "OptionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Contents_ContentId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_InstructorId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Contents_ContentId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Levels_LevelId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Sections_SectionId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_InstructorId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_SelectedOptionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_AttemptId_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ContentId_SectionId_LevelId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_QuizType",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_QuizId_OrderIndex",
                table: "QuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_QuizId_QuestionId",
                table: "QuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId_UserId_AttemptNumber",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuestionType",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_QuestionId_OrderIndex",
                table: "QuestionOptions");

            migrationBuilder.AlterColumn<int>(
                name: "PassingScore",
                table: "Quizzes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 70);

            migrationBuilder.AlterColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 3);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasCode",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "OptionText",
                table: "QuestionOptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAnswer_AnswerType",
                table: "UserAnswers",
                sql: "(SelectedOptionId IS NOT NULL AND BooleanAnswer IS NULL) OR (SelectedOptionId IS NULL AND BooleanAnswer IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ContentId",
                table: "Quizzes",
                column: "ContentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quiz_HierarchyConstraint",
                table: "Quizzes",
                sql: "(QuizType = 1 AND ContentId IS NOT NULL AND SectionId IS NULL AND LevelId IS NULL) OR  \r\n                     (QuizType = 2 AND SectionId IS NOT NULL AND ContentId IS NULL AND LevelId IS NULL) OR  \r\n                     (QuizType = 3 AND LevelId IS NOT NULL AND ContentId IS NULL AND SectionId IS NULL) OR  \r\n                     (QuizType = 4 AND ContentId IS NULL AND SectionId IS NULL AND LevelId IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId",
                table: "QuizQuestions",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Contents_ContentId",
                table: "Questions",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_InstructorId",
                table: "Questions",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Contents_ContentId",
                table: "Quizzes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Levels_LevelId",
                table: "Quizzes",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Sections_SectionId",
                table: "Quizzes",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_InstructorId",
                table: "Quizzes",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_SelectedOptionId",
                table: "UserAnswers",
                column: "SelectedOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "OptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
