using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceStudentFeaturesConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Courses_CourseId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningAnalytics_Users_UserId",
                table: "UserLearningAnalytics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningStreaks_Users_UserId",
                table: "UserLearningStreaks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Achievements_AchievementId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Contents_ContentId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Courses_CourseId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserLearningStreaks_UserId",
                table: "UserLearningStreaks");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "UserNotifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Normal",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "UserNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserNotifications",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "WeeklyGoalDays",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LongestStreak",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserLearningStreaks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsStreakActive",
                table: "UserLearningStreaks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "HasMetWeeklyGoal",
                table: "UserLearningStreaks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentWeekDays",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStreak",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "SendReminders",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAchieved",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserLearningGoals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPointsEarned",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "MetWeeklyGoal",
                table: "UserLearningAnalytics",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "MetDailyGoal",
                table: "UserLearningAnalytics",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserLearningAnalytics",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "DailySessions",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DailyLearningMinutes",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DailyContentCompleted",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyAverageSessionLength",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionRate",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageQuizScore",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AnalyticsDate",
                table: "UserLearningAnalytics",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserBookmarks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookmarkedAt",
                table: "UserBookmarks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "PointsAwarded",
                table: "UserAchievements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EarnedAt",
                table: "UserAchievements",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "UserAchievements",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRare",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultPoints",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Achievements",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_CreatedAt",
                table: "UserNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_IsRead",
                table: "UserNotifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_Type",
                table: "UserNotifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningStreaks_UserId",
                table: "UserLearningStreaks",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningGoals_IsActive",
                table: "UserLearningGoals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningAnalytics_AnalyticsDate",
                table: "UserLearningAnalytics",
                column: "AnalyticsDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningAnalytics_UserId_AnalyticsDate",
                table: "UserLearningAnalytics",
                columns: new[] { "UserId", "AnalyticsDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBookmarks_UserId_ContentId",
                table: "UserBookmarks",
                columns: new[] { "UserId", "ContentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_CourseId1",
                table: "UserAchievements",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Category",
                table: "Achievements",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_IsActive",
                table: "Achievements",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "AchievementId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Courses_CourseId",
                table: "UserAchievements",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Courses_CourseId1",
                table: "UserAchievements",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningAnalytics_Users_UserId",
                table: "UserLearningAnalytics",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningStreaks_Users_UserId",
                table: "UserLearningStreaks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Achievements_AchievementId",
                table: "UserNotifications",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "AchievementId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Contents_ContentId",
                table: "UserNotifications",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Courses_CourseId",
                table: "UserNotifications",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Courses_CourseId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Courses_CourseId1",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningAnalytics_Users_UserId",
                table: "UserLearningAnalytics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningStreaks_Users_UserId",
                table: "UserLearningStreaks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Achievements_AchievementId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Contents_ContentId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Courses_CourseId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_CreatedAt",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_IsRead",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_Type",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserLearningStreaks_UserId",
                table: "UserLearningStreaks");

            migrationBuilder.DropIndex(
                name: "IX_UserLearningGoals_IsActive",
                table: "UserLearningGoals");

            migrationBuilder.DropIndex(
                name: "IX_UserLearningAnalytics_AnalyticsDate",
                table: "UserLearningAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_UserLearningAnalytics_UserId_AnalyticsDate",
                table: "UserLearningAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_UserBookmarks_UserId_ContentId",
                table: "UserBookmarks");

            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_CourseId1",
                table: "UserAchievements");

            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_Category",
                table: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_IsActive",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "UserAchievements");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "UserNotifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Normal");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "UserNotifications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserNotifications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "WeeklyGoalDays",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<int>(
                name: "LongestStreak",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserLearningStreaks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsStreakActive",
                table: "UserLearningStreaks",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "HasMetWeeklyGoal",
                table: "UserLearningStreaks",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentWeekDays",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStreak",
                table: "UserLearningStreaks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "SendReminders",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAchieved",
                table: "UserLearningGoals",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserLearningGoals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPointsEarned",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "MetWeeklyGoal",
                table: "UserLearningAnalytics",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "MetDailyGoal",
                table: "UserLearningAnalytics",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserLearningAnalytics",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "DailySessions",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DailyLearningMinutes",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DailyContentCompleted",
                table: "UserLearningAnalytics",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyAverageSessionLength",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionRate",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageQuizScore",
                table: "UserLearningAnalytics",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AnalyticsDate",
                table: "UserLearningAnalytics",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserBookmarks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookmarkedAt",
                table: "UserBookmarks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "PointsAwarded",
                table: "UserAchievements",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EarnedAt",
                table: "UserAchievements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRare",
                table: "Achievements",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Achievements",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultPoints",
                table: "Achievements",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Achievements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningStreaks_UserId",
                table: "UserLearningStreaks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "AchievementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Courses_CourseId",
                table: "UserAchievements",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningAnalytics_Users_UserId",
                table: "UserLearningAnalytics",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningStreaks_Users_UserId",
                table: "UserLearningStreaks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Achievements_AchievementId",
                table: "UserNotifications",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "AchievementId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Contents_ContentId",
                table: "UserNotifications",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Courses_CourseId",
                table: "UserNotifications",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}
