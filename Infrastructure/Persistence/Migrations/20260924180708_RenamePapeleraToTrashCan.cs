using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class RenamePapeleraToTrashCan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Papelera_StoredFiles_FileId", "Papelera");
            migrationBuilder.DropPrimaryKey("PK_Papelera", "Papelera");
            migrationBuilder.RenameTable("Papelera", newName: "TrashCan");
            migrationBuilder.RenameIndex(
                name: "IX_Papelera_PurgeAt",
                table: "TrashCan",
                newName: "IX_TrashCan_PurgeAt");
            migrationBuilder.AddPrimaryKey("PK_TrashCan", "TrashCan", "FileId");
            migrationBuilder.AddForeignKey(
                name: "FK_TrashCan_StoredFiles_FileId",
                table: "TrashCan",
                column: "FileId",
                principalTable: "StoredFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_TrashCan_StoredFiles_FileId", "TrashCan");
            migrationBuilder.DropPrimaryKey("PK_TrashCan", "TrashCan");
            migrationBuilder.RenameTable("TrashCan", newName: "Papelera");
            migrationBuilder.RenameIndex(
                name: "IX_TrashCan_PurgeAt",
                table: "Papelera",
                newName: "IX_Papelera_PurgeAt");
            migrationBuilder.AddPrimaryKey("PK_Papelera", "Papelera", "FileId");
            migrationBuilder.AddForeignKey(
                name: "FK_Papelera_StoredFiles_FileId",
                table: "Papelera",
                column: "FileId",
                principalTable: "StoredFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
