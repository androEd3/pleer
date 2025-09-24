using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pleer.Migrations
{
    /// <inheritdoc />
    public partial class reformSongClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SongPath",
                table: "Songs",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "AlbumCoverPath",
                table: "AlbumCovers",
                newName: "FilePath");

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Songs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Songs");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Songs",
                newName: "SongPath");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "AlbumCovers",
                newName: "AlbumCoverPath");
        }
    }
}
