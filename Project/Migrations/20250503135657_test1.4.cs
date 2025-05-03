using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class test14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Feeling",
                table: "FeedbackComments",
                newName: "CommentRate");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "FeedbackComments",
                newName: "TranslateComment");

            migrationBuilder.AddColumn<string>(
                name: "OriginalComment",
                table: "FeedbackComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalComment",
                table: "FeedbackComments");

            migrationBuilder.RenameColumn(
                name: "TranslateComment",
                table: "FeedbackComments",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "CommentRate",
                table: "FeedbackComments",
                newName: "Feeling");
        }
    }
}
