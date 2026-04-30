using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdegaOak.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bebida = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tamanho = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Material = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorVenda = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuantidadeCaixa = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorCaixa = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorAtacadoCaixa = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EstoqueMinimo = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeMinimaAtacado = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaldoConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CapitalAdmin = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaldoConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true),
                    PrecoVenda = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    EhCopao = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Pago = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notas = table.Column<string>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despesas_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Responsavel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorDinheiro = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorCartao = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorPix = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Movimentacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TipoVenda = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoDescricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Responsavel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TipoSaida = table.Column<string>(type: "TEXT", nullable: true),
                    ValorUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimentacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComboComposicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComboId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DebitaEstoque = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboComposicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboComposicoes_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboComposicoes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComboVendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComboId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecoTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Responsavel = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    TipoMovimento = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboVendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboVendas_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComboVendas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboComposicoes_ComboId",
                table: "ComboComposicoes",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboComposicoes_ProdutoId",
                table: "ComboComposicoes",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_Nome",
                table: "Combos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboVendas_ComboId",
                table: "ComboVendas",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboVendas_UsuarioId",
                table: "ComboVendas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_ProdutoId",
                table: "Despesas",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_ProdutoId",
                table: "Movimentacoes",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_UsuarioId",
                table: "Movimentacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_VendaId",
                table: "Movimentacoes",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_Tipo_Data",
                table: "Movimentacoes",
                columns: new[] { "Tipo", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_Data",
                table: "Movimentacoes",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Ativo",
                table: "Produtos",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_Data",
                table: "Despesas",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_Pago_Data",
                table: "Despesas",
                columns: new[] { "Pago", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_Data",
                table: "Vendas",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioId",
                table: "Vendas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Username",
                table: "Usuarios",
                column: "Username",
                unique: true);

            migrationBuilder.InsertData(
                table: "SaldoConfigs",
                columns: new[] { "Id", "CapitalAdmin", "AtualizadoEm" },
                values: new object[] { 1, 0m, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Nome", "Username", "PasswordHash", "Role", "Ativo", "CriadoEm" },
                values: new object[] { 1, "Administrador", "admin", "$2a$11$8vZ7YQfyVZxH5L3qN9X8/.rKZJ5mHJxGxN8fQXqZ5vZ7YQfyVZxH5", "admin", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ComboComposicoes");
            migrationBuilder.DropTable(name: "ComboVendas");
            migrationBuilder.DropTable(name: "Combos");
            migrationBuilder.DropTable(name: "Despesas");
            migrationBuilder.DropTable(name: "Movimentacoes");
            migrationBuilder.DropTable(name: "Vendas");
            migrationBuilder.DropTable(name: "Produtos");
            migrationBuilder.DropTable(name: "Usuarios");
            migrationBuilder.DropTable(name: "SaldoConfigs");
        }
    }
}
