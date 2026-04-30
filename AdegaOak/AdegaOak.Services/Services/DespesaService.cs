using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;

namespace AdegaOak.Services.Services;

public class DespesaService(
    IDespesaRepository despesaRepository,
    IProdutoRepository produtoRepository,
    IMovimentacaoRepository movimentacaoRepository) : IDespesaService
{
    private static readonly Dictionary<TipoDespesa, string> TipoNomes = new()
    {
        { TipoDespesa.Aluguel, "Aluguel" },
        { TipoDespesa.Salario, "Salário" },
        { TipoDespesa.Luz, "Luz" },
        { TipoDespesa.Agua, "Água" },
        { TipoDespesa.Internet, "Internet" },
        { TipoDespesa.Manutencao, "Manutenção" },
        { TipoDespesa.Limpeza, "Limpeza" },
        { TipoDespesa.Publicidade, "Publicidade" },
        { TipoDespesa.Transporte, "Transporte" },
        { TipoDespesa.Outros, "Outros" },
        { TipoDespesa.Consumo, "Consumo" },
        { TipoDespesa.Vencimento, "Vencimento" },
        { TipoDespesa.Avaria, "Avaria" },
        { TipoDespesa.ConsumoCopoes, "Consumo Copões" }
    };

    public async Task<List<DespesaDto>> GetAllAsync()
    {
        var despesas = await despesaRepository.GetAllAsync();
        return despesas.Select(MapToDto).ToList();
    }

    public async Task<DespesaDto?> GetByIdAsync(int id)
    {
        var despesa = await despesaRepository.GetByIdAsync(id);
        return despesa == null ? null : MapToDto(despesa);
    }

    public async Task<DespesaDto> CreateAsync(CreateDespesaRequest request, int usuarioId, string responsavel)
    {
        var despesa = new Despesa
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            Data = request.Data,
            Tipo = request.Tipo,
            Notas = request.Notas,
            ProdutoId = request.ProdutoId,
            Quantidade = request.Quantidade
        };

        await despesaRepository.CreateAsync(despesa);

        // If product-linked, create Saída movement
        if (request.ProdutoId.HasValue && request.Quantidade > 0)
        {
            var produto = await produtoRepository.GetByIdAsync(request.ProdutoId.Value)
                ?? throw new KeyNotFoundException($"Produto {request.ProdutoId} não encontrado.");

            var movimentacao = new Movimentacao
            {
                Data = request.Data,
                Tipo = "Saída",
                TipoVenda = "Varejo",
                ProdutoId = request.ProdutoId.Value,
                ProdutoDescricao = produto.Descricao,
                Quantidade = request.Quantidade,
                UsuarioId = usuarioId,
                Responsavel = responsavel,
                TipoSaida = TipoNomes[request.Tipo],
                ValorUnitario = 0
            };

            await movimentacaoRepository.CreateAsync(movimentacao);
        }

        return MapToDto(despesa);
    }

    public async Task<DespesaDto> UpdateAsync(int id, UpdateDespesaRequest request)
    {
        var despesa = await despesaRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Despesa {id} não encontrada.");

        if (request.Descricao != null) despesa.Descricao = request.Descricao;
        if (request.Valor.HasValue) despesa.Valor = request.Valor.Value;
        if (request.Data.HasValue) despesa.Data = request.Data.Value;
        if (request.Tipo.HasValue) despesa.Tipo = request.Tipo.Value;
        if (request.Notas != null) despesa.Notas = request.Notas;

        await despesaRepository.UpdateAsync(despesa);
        return MapToDto(despesa);
    }

    public async Task DeleteAsync(int id) =>
        await despesaRepository.DeleteAsync(id);

    public async Task<List<DespesaDto>> GetByPeriodoAsync(int mes, int ano)
    {
        var despesas = await despesaRepository.GetByPeriodoAsync(mes, ano);
        return despesas.Select(MapToDto).ToList();
    }

    public async Task MarcarPagaAsync(int id, bool pago) =>
        await despesaRepository.MarcarPagaAsync(id, pago);

    public async Task<DespesaResumoDto> GetResumoAsync(int? mes = null, int? ano = null)
    {
        var despesas = mes.HasValue && ano.HasValue
            ? await despesaRepository.GetByPeriodoAsync(mes.Value, ano.Value)
            : await despesaRepository.GetAllAsync();

        var totalPago = despesas.Where(d => d.Pago).Sum(d => (decimal?)d.Valor) ?? 0;
        var totalPendente = despesas.Where(d => !d.Pago).Sum(d => (decimal?)d.Valor) ?? 0;

        return new DespesaResumoDto(
            totalPago,
            totalPendente,
            totalPago + totalPendente,
            despesas.Count(d => d.Pago),
            despesas.Count(d => !d.Pago)
        );
    }

    private static DespesaDto MapToDto(Despesa d) =>
        new(
            d.Id,
            d.Descricao,
            d.Valor,
            d.Data,
            d.Tipo,
            TipoNomes[d.Tipo],
            d.Pago,
            d.DataPagamento,
            d.Notas,
            d.ProdutoId,
            d.Produto?.Descricao,
            d.Quantidade,
            d.CriadoEm
        );
}
