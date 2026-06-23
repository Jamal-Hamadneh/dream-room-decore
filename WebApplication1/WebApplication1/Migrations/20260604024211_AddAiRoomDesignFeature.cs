using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddAiRoomDesignFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ai_description",
                table: "room_uploads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ai_detected_depth",
                table: "room_uploads",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ai_detected_height",
                table: "room_uploads",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ai_detected_width",
                table: "room_uploads",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "room_furniture_placements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "z_index",
                table: "room_furniture_placements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "generated_room_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_design_id = table.Column<int>(type: "int", nullable: false),
                    generated_image_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ai_analysis_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_generated_room_images", x => x.id);
                    table.CheckConstraint("ck_generated_room_images_status", "status IN ('pending', 'completed', 'failed')");
                    table.ForeignKey(
                        name: "fk_generated_room_images__room_designs_room_design_id",
                        column: x => x.room_design_id,
                        principalTable: "room_designs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "room_design_replacements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_design_id = table.Column<int>(type: "int", nullable: false),
                    old_product_id = table.Column<int>(type: "int", nullable: false),
                    new_product_id = table.Column<int>(type: "int", nullable: false),
                    instruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    generated_image_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_room_design_replacements", x => x.id);
                    table.CheckConstraint("ck_room_design_replacements_status", "status IN ('pending', 'completed', 'failed')");
                    table.ForeignKey(
                        name: "fk_room_design_replacements_products_new_product_id",
                        column: x => x.new_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_room_design_replacements_products_old_product_id",
                        column: x => x.old_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_room_design_replacements_room_designs_room_design_id",
                        column: x => x.room_design_id,
                        principalTable: "room_designs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_generated_room_images_room_design_id",
                table: "generated_room_images",
                column: "room_design_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_design_replacements_new_product_id",
                table: "room_design_replacements",
                column: "new_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_design_replacements_old_product_id",
                table: "room_design_replacements",
                column: "old_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_design_replacements_room_design_id",
                table: "room_design_replacements",
                column: "room_design_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "generated_room_images");

            migrationBuilder.DropTable(
                name: "room_design_replacements");

            migrationBuilder.DropColumn(
                name: "ai_description",
                table: "room_uploads");

            migrationBuilder.DropColumn(
                name: "ai_detected_depth",
                table: "room_uploads");

            migrationBuilder.DropColumn(
                name: "ai_detected_height",
                table: "room_uploads");

            migrationBuilder.DropColumn(
                name: "ai_detected_width",
                table: "room_uploads");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "room_furniture_placements");

            migrationBuilder.DropColumn(
                name: "z_index",
                table: "room_furniture_placements");
        }
    }
}
