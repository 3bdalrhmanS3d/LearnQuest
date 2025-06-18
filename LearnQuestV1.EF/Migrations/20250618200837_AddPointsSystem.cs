using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnQuestV1.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoursePoints",
                columns: table => new
                {
                    CoursePointsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    QuizPoints = table.Column<int>(type: "int", nullable: false),
                    BonusPoints = table.Column<int>(type: "int", nullable: false),
                    PenaltyPoints = table.Column<int>(type: "int", nullable: false),
                    CurrentRank = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePoints", x => x.CoursePointsId);
                    table.ForeignKey(
                        name: "FK_CoursePoints_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CoursePoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CoursePointsId = table.Column<int>(type: "int", nullable: false),
                    PointsChanged = table.Column<int>(type: "int", nullable: false),
                    PointsAfterTransaction = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    QuizAttemptId = table.Column<int>(type: "int", nullable: true),
                    AwardedByUserId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_PointTransactions_CoursePoints_CoursePointsId",
                        column: x => x.CoursePointsId,
                        principalTable: "CoursePoints",
                        principalColumn: "CoursePointsId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PointTransactions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PointTransactions_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "AttemptId");
                    table.ForeignKey(
                        name: "FK_PointTransactions_Users_AwardedByUserId",
                        column: x => x.AwardedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PointTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursePoints_CourseId",
                table: "CoursePoints",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePoints_UserId",
                table: "CoursePoints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_AwardedByUserId",
                table: "PointTransactions",
                column: "AwardedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CourseId",
                table: "PointTransactions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CoursePointsId",
                table: "PointTransactions",
                column: "CoursePointsId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_QuizAttemptId",
                table: "PointTransactions",
                column: "QuizAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId",
                table: "PointTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "CoursePoints");
        }
    }
}
