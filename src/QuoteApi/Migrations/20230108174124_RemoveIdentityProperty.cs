using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuoteApi.Migrations
{
    public partial class RemoveIdentityProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuoteCreatorNormalized",
                table: "Quotes",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValueSql: "upper(QuoteCreator)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuoteCreatorNormalized",
                table: "Quotes",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "upper(QuoteCreator)",
                oldClrType: typeof(string));
        }
    }
}
