using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearMonth = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmissionNumber = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<int>(type: "int", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResubmitDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Process = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonCostDown = table.Column<bool>(type: "bit", nullable: false),
                    ReasonAlternativeSourcing = table.Column<bool>(type: "bit", nullable: false),
                    ReasonOthers = table.Column<bool>(type: "bit", nullable: false),
                    ReasonOthersText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Merit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Demerit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialDisposition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleQuantity = table.Column<int>(type: "int", nullable: true),
                    TestLotType = table.Column<int>(type: "int", nullable: true),
                    TestLotDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestLotCodeSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicableToChemicalOrMaterials = table.Column<bool>(type: "bit", nullable: false),
                    SafetyDataSheet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalClassification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalInventorySystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedBySafetyUserId = table.Column<int>(type: "int", nullable: true),
                    CheckedByDeptHeadUserId = table.Column<int>(type: "int", nullable: true),
                    CheckedByQaUserId = table.Column<int>(type: "int", nullable: true),
                    SafetyDecision = table.Column<int>(type: "int", nullable: false),
                    DeptHeadDecision = table.Column<int>(type: "int", nullable: false),
                    QaDecision = table.Column<int>(type: "int", nullable: false),
                    CooDecision = table.Column<int>(type: "int", nullable: false),
                    SafetyComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeptHeadComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QaComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafetyDecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeptHeadDecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QaDecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CooDecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByCooUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineeringRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineeringRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringRequestId = table.Column<int>(type: "int", nullable: false),
                    ToEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailLogs_EngineeringRequests_EngineeringRequestId",
                        column: x => x.EngineeringRequestId,
                        principalTable: "EngineeringRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringRequestId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErAttachments_EngineeringRequests_EngineeringRequestId",
                        column: x => x.EngineeringRequestId,
                        principalTable: "EngineeringRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringRequestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErComments_EngineeringRequests_EngineeringRequestId",
                        column: x => x.EngineeringRequestId,
                        principalTable: "EngineeringRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ErComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EngineeringRequestId",
                table: "EmailLogs",
                column: "EngineeringRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringRequests_ErNumber_SubmissionNumber",
                table: "EngineeringRequests",
                columns: new[] { "ErNumber", "SubmissionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringRequests_RequesterId",
                table: "EngineeringRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ErAttachments_EngineeringRequestId",
                table: "ErAttachments",
                column: "EngineeringRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ErComments_EngineeringRequestId",
                table: "ErComments",
                column: "EngineeringRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ErComments_UserId",
                table: "ErComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErSequences_YearMonth",
                table: "ErSequences",
                column: "YearMonth",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupItems_Category_Value",
                table: "LookupItems",
                columns: new[] { "Category", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "ErAttachments");

            migrationBuilder.DropTable(
                name: "ErComments");

            migrationBuilder.DropTable(
                name: "ErSequences");

            migrationBuilder.DropTable(
                name: "LookupItems");

            migrationBuilder.DropTable(
                name: "EngineeringRequests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
