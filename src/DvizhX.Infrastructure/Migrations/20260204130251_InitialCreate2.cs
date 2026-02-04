using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DvizhX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Columns_BoardColumnId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_BoardColumnId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "BoardColumnId",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ColumnId",
                table: "Cards",
                column: "ColumnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Columns_ColumnId",
                table: "Cards",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Columns_ColumnId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ColumnId",
                table: "Cards");

            migrationBuilder.AddColumn<Guid>(
                name: "BoardColumnId",
                table: "Cards",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_BoardColumnId",
                table: "Cards",
                column: "BoardColumnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Columns_BoardColumnId",
                table: "Cards",
                column: "BoardColumnId",
                principalTable: "Columns",
                principalColumn: "Id");
        }
    }
}
