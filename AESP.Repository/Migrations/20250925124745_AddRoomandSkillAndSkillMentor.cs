using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomandSkillAndSkillMentor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "PracticeSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "GroupSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "SkillMentors",
                columns: table => new
                {
                    SkillMentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsExpertised = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillMentors", x => x.SkillMentorId);
                    table.ForeignKey(
                        name: "FK_SkillMentors_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "MentorProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillMentors_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_RoomId",
                table: "PracticeSessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSessions_RoomId",
                table: "GroupSessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillMentors_MentorProfileId",
                table: "SkillMentors",
                column: "MentorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillMentors_SkillId",
                table: "SkillMentors",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSessions_Rooms_RoomId",
                table: "GroupSessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PracticeSessions_Rooms_RoomId",
                table: "PracticeSessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSessions_Rooms_RoomId",
                table: "GroupSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_PracticeSessions_Rooms_RoomId",
                table: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "SkillMentors");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_PracticeSessions_RoomId",
                table: "PracticeSessions");

            migrationBuilder.DropIndex(
                name: "IX_GroupSessions_RoomId",
                table: "GroupSessions");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "GroupSessions");
        }
    }
}
