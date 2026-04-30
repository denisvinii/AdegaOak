using AdegaOak.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AdegaOak.Data.Data;

public class AdegaOakDbContext(DbContextOptions<AdegaOakDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<Combo> Combos => Set<Combo>();
    public DbSet<ComboComposicao> ComboComposicoes => Set<ComboComposicao>();
    public DbSet<ComboVenda> ComboVendas => Set<ComboVenda>();
    public DbSet<SaldoConfig> SaldoConfigs => Set<SaldoConfig>();
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Usuario
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Nome).IsRequired().HasMaxLength(100);
            e.Property(u => u.Username).IsRequired().HasMaxLength(50);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        // Produto
        modelBuilder.Entity<Produto>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Bebida).IsRequired().HasMaxLength(100);
            e.Property(p => p.Tamanho).IsRequired().HasMaxLength(50);
            e.Property(p => p.Material).IsRequired().HasMaxLength(50);
            e.Property(p => p.Valor).HasColumnType("decimal(10,2)");
            e.Property(p => p.ValorVenda).HasColumnType("decimal(10,2)");
            e.Property(p => p.ValorCaixa).HasColumnType("decimal(10,2)");
            e.Property(p => p.ValorAtacadoCaixa).HasColumnType("decimal(10,2)");
            e.Ignore(p => p.Descricao); // computed property
            
            // Performance index for active products
            e.HasIndex(p => p.Ativo).HasDatabaseName("IX_Produtos_Ativo");
        });

        // Movimentacao
        modelBuilder.Entity<Movimentacao>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Tipo).IsRequired().HasMaxLength(10);
            e.Property(m => m.TipoVenda).IsRequired().HasMaxLength(10);
            e.Property(m => m.ProdutoDescricao).IsRequired().HasMaxLength(200);
            e.Property(m => m.Responsavel).IsRequired().HasMaxLength(100);
            e.Property(m => m.ValorUnitario).HasColumnType("decimal(10,2)");
            e.Ignore(m => m.ValorTotal); // computed property
            e.HasOne(m => m.Produto).WithMany(p => p.Movimentacoes).HasForeignKey(m => m.ProdutoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Usuario).WithMany(u => u.Movimentacoes).HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Venda).WithMany(v => v.Movimentacoes).HasForeignKey(m => m.VendaId).OnDelete(DeleteBehavior.SetNull);
            
            // Performance indexes for dashboard queries
            e.HasIndex(m => new { m.Tipo, m.Data }).HasDatabaseName("IX_Movimentacoes_Tipo_Data");
            e.HasIndex(m => m.Data).HasDatabaseName("IX_Movimentacoes_Data");
            e.HasIndex(m => m.ProdutoId).HasDatabaseName("IX_Movimentacoes_ProdutoId");
            e.HasIndex(m => m.UsuarioId).HasDatabaseName("IX_Movimentacoes_UsuarioId");
            e.HasIndex(m => m.VendaId).HasDatabaseName("IX_Movimentacoes_VendaId");
        });

        // Despesa
        modelBuilder.Entity<Despesa>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Descricao).IsRequired().HasMaxLength(200);
            e.Property(d => d.Valor).HasColumnType("decimal(10,2)");
            e.HasOne(d => d.Produto).WithMany(p => p.Despesas).HasForeignKey(d => d.ProdutoId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            
            // Performance indexes for dashboard queries
            e.HasIndex(d => d.Data).HasDatabaseName("IX_Despesas_Data");
            e.HasIndex(d => new { d.Pago, d.Data }).HasDatabaseName("IX_Despesas_Pago_Data");
        });

        // Combo
        modelBuilder.Entity<Combo>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Nome).IsUnique();
            e.Property(c => c.Nome).IsRequired().HasMaxLength(100);
            e.Property(c => c.PrecoVenda).HasColumnType("decimal(10,2)");
        });

        // ComboComposicao
        modelBuilder.Entity<ComboComposicao>(e =>
        {
            e.HasKey(cc => cc.Id);
            e.Property(cc => cc.Quantidade).HasColumnType("decimal(10,4)");
            e.Property(cc => cc.Unidade).IsRequired().HasMaxLength(20);
            e.HasOne(cc => cc.Combo).WithMany(c => c.Composicao).HasForeignKey(cc => cc.ComboId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cc => cc.Produto).WithMany(p => p.ComboComposicoes).HasForeignKey(cc => cc.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ComboVenda
        modelBuilder.Entity<ComboVenda>(e =>
        {
            e.HasKey(cv => cv.Id);
            e.Property(cv => cv.PrecoUnitario).HasColumnType("decimal(10,2)");
            e.Property(cv => cv.PrecoTotal).HasColumnType("decimal(10,2)");
            e.HasOne(cv => cv.Combo).WithMany(c => c.Vendas).HasForeignKey(cv => cv.ComboId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(cv => cv.Usuario).WithMany(u => u.ComboVendas).HasForeignKey(cv => cv.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        // SaldoConfig
        modelBuilder.Entity<SaldoConfig>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.CapitalAdmin).HasColumnType("decimal(10,2)");
        });

        // Venda
        modelBuilder.Entity<Venda>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Responsavel).IsRequired().HasMaxLength(100);
            e.Property(v => v.ValorTotal).HasColumnType("decimal(10,2)");
            e.Property(v => v.ValorDinheiro).HasColumnType("decimal(10,2)");
            e.Property(v => v.ValorCartao).HasColumnType("decimal(10,2)");
            e.Property(v => v.ValorPix).HasColumnType("decimal(10,2)");
            e.HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            
            // Performance indexes
            e.HasIndex(v => v.Data).HasDatabaseName("IX_Vendas_Data");
            e.HasIndex(v => v.UsuarioId).HasDatabaseName("IX_Vendas_UsuarioId");
        });

        // NOTE: Admin user is NOT seeded here.
        // It is created at startup by DatabaseSeeder, which hashes the
        // password at runtime from configuration (Seed:AdminPassword).
        modelBuilder.Entity<SaldoConfig>().HasData(new SaldoConfig
        {
            Id = 1,
            CapitalAdmin = 0,
            AtualizadoEm = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
