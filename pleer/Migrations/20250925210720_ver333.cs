using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pleer.Migrations
{
    /// <inheritdoc />
    public partial class ver333 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationSeconds",
                table: "Songs",
                newName: "TotalDuration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalDuration",
                table: "Songs",
                newName: "DurationSeconds");
        }
    }
}
