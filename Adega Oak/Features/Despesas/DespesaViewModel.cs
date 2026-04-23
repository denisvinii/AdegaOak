using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System;
using System.Text;

namespace Adega_Oak.Features.Despesas;

public partial class DespesaViewModel : ObservableObject
{
    private readonly DespesaRepository _despesaRepository;
    private readonly SaldoRepository _saldoRepository;
    private readonly MainRepository _mainRepository;

    [ObservableProperty]
    private ObservableCollection<DespesaRepository.Despesa> despesas = new();

    [ObservableProperty]
    private decimal totalDespesasNaoPagas;

    [ObservableProperty]
    private decimal totalDespesasPagas;

    [ObservableProperty]
    private decimal saldoAtualizado;

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private decimal valor;

    [ObservableProperty]
    private DateTime dataDespesa = DateTime.Now;

    [ObservableProperty]
    private DespesaRepository.TipoDespesa tipoDespesaSelecionado = DespesaRepository.TipoDespesa.Outros;

    // Index to bind ComboBox SelectedIndex (keeps TipoDespesaSelecionado in sync)
    private int _tipoDespesaIndex = 9; // default to Outros (index 9 based on enum order)
    public int TipoDespesaIndex
    {
        get => _tipoDespesaIndex;
        set
        {
            if (SetProperty(ref _tipoDespesaIndex, value))
            {
                // map index to enum (safety: clamp)
                var values = System.Enum.GetValues(typeof(DespesaRepository.TipoDespesa)).Cast<DespesaRepository.TipoDespesa>().ToArray();
                if (value >= 0 && value < values.Length)
                {
                    TipoDespesaSelecionado = values[value];
                }
            }
        }
    }

    [ObservableProperty]
    private string notas = string.Empty;

    [ObservableProperty]
    private int mesSelecionado = DateTime.Now.Month;

    [ObservableProperty]
    private int anoSelecionado = DateTime.Now.Year;

    [ObservableProperty]
    private ObservableCollection<int> meses = new(Enumerable.Range(1, 12));

    [ObservableProperty]
    private ObservableCollection<int> anos = new(Enumerable.Range(DateTime.Now.Year - 0, 10));

    [ObservableProperty]
    private ObservableCollection<string> tiposDespesa = new();

    [ObservableProperty]
    private DespesaRepository.Despesa? despesaSelecionada;

    [ObservableProperty]
    private bool emEdicao;

    // Produtos (explicit properties to avoid source-generator timing issues)
    private ObservableCollection<MainRepository.Produto> _produtos = new();
    public ObservableCollection<MainRepository.Produto> Produtos
    {
        get => _produtos;
        set => SetProperty(ref _produtos, value);
    }

    private MainRepository.Produto? _produtoSelecionado;
    public MainRepository.Produto? ProdutoSelecionado
    {
        get => _produtoSelecionado;
        set => SetProperty(ref _produtoSelecionado, value);
    }

    private int _quantidadeProduto = 1;
    public int QuantidadeProduto
    {
        get => _quantidadeProduto;
        set => SetProperty(ref _quantidadeProduto, value);
    }

    private bool _mostrarProdutos = false;
    public bool MostrarProdutos
    {
        get => _mostrarProdutos;
        set => SetProperty(ref _mostrarProdutos, value);
    }

    // Products filtered and search text (explicit to control setter)
    private ObservableCollection<MainRepository.Produto> _produtosFiltrados = new();
    public ObservableCollection<MainRepository.Produto> ProdutosFiltrados
    {
        get => _produtosFiltrados;
        set => SetProperty(ref _produtosFiltrados, value);
    }

    private string? _produtoFiltroTexto;
    public string? ProdutoFiltroTexto
    {
        get => _produtoFiltroTexto;
        set
        {
            if (SetProperty(ref _produtoFiltroTexto, value))
            {
                FiltrarProdutos(value);
            }
        }
    }

    public DespesaViewModel(DespesaRepository despesaRepository, SaldoRepository saldoRepository, MainRepository mainRepository)
    {
        _despesaRepository = despesaRepository;
        _saldoRepository = saldoRepository;
        _mainRepository = mainRepository;

        InicializarTipos();
        CarregarDespesas();
        AtualizarTotais();
        AtualizarSaldo();

        // Carrega produtos para seleção
        var produtosCarregados = _mainRepository.CarregarProdutos();
        Produtos = new ObservableCollection<MainRepository.Produto>(produtosCarregados);
        ProdutosFiltrados = new ObservableCollection<MainRepository.Produto>(Produtos);
    }

    // Ensure when enum property changes (e.g., programmatically) we update index
    partial void OnTipoDespesaSelecionadoChanged(DespesaRepository.TipoDespesa value)
    {
        TipoDespesaIndex = (int)value;
        // Mostrar controles de produtos para tipos Consumo, Vencimento e Avaria
        MostrarProdutos = value == DespesaRepository.TipoDespesa.Consumo
                        || value == DespesaRepository.TipoDespesa.Vencimento
                        || value == DespesaRepository.TipoDespesa.Avaria;
    }

    private void InicializarTipos()
    {
        TiposDespesa.Clear();
        foreach (DespesaRepository.TipoDespesa tipo in Enum.GetValues(typeof(DespesaRepository.TipoDespesa)))
        {
            TiposDespesa.Add(DespesaRepository.TipoDespesaHelper.GetDisplayName(tipo));
        }
    }

    [RelayCommand]
    public void CarregarDespesas()
    {
        Despesas.Clear();
        var despesasCarregadas = _despesaRepository.CarregarTodasDespesas();
        foreach (var d in despesasCarregadas.OrderByDescending(x => x.Data))
            Despesas.Add(d);
        AtualizarTotais();
    }

    [RelayCommand]
    public void AdicionarDespesa()
    {
        // Descricao always required
        if (string.IsNullOrWhiteSpace(Descricao))
        {
            MessageBox.Show("Por favor, preencha a descrição.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Valor é obrigatório apenas quando NÃO é despesa ligada a produto
        if (!MostrarProdutos && Valor <= 0)
        {
            MessageBox.Show("Por favor, preencha um valor válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var despesa = new DespesaRepository.Despesa
        {
            Descricao = Descricao,
            // Se for despesa de produto, força valor 0
            Valor = MostrarProdutos ? 0m : Valor,
            Data = DataDespesa,
            Tipo = TipoDespesaSelecionado,
            Notas = Notas,
            Pago = false,
            CriadoEm = DateTime.Now
        };

        if (MostrarProdutos)
        {
            if (ProdutoSelecionado is null)
            {
                MessageBox.Show("Selecione um produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (QuantidadeProduto <= 0)
            {
                MessageBox.Show("Quantidade inválida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            despesa.ProductId = ProdutoSelecionado.ProductId;
            despesa.Quantidade = QuantidadeProduto;
            despesa.Descricao = $"{DespesaRepository.TipoDespesaHelper.GetDisplayName(despesa.Tipo)} - {ProdutoSelecionado.Descricao}";
        }

        _despesaRepository.AdicionarDespesa(despesa);
        LimparFormulario();
        CarregarDespesas();
        AtualizarSaldo();
        MessageBox.Show("Despesa adicionada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    public void EditarDespesa()
    {
        if (DespesaSelecionada is null)
        {
            MessageBox.Show("Selecione uma despesa para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EmEdicao = true;
        Descricao = DespesaSelecionada.Descricao;
        Valor = DespesaSelecionada.Valor;
        DataDespesa = DespesaSelecionada.Data;
        TipoDespesaSelecionado = DespesaSelecionada.Tipo;
        Notas = DespesaSelecionada.Notas ?? string.Empty;

        if (DespesaSelecionada.ProductId.HasValue)
        {
            ProdutoSelecionado = Produtos.FirstOrDefault(p => p.ProductId == DespesaSelecionada.ProductId.Value);
            QuantidadeProduto = DespesaSelecionada.Quantidade;
        }
    }

    [RelayCommand]
    public void SalvarEdicao()
    {
        if (DespesaSelecionada is null)
        {
            MessageBox.Show("Nenhuma despesa selecionada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        DespesaSelecionada.Descricao = Descricao;
        // Se for despesa de produto, garante valor = 0
        DespesaSelecionada.Valor = MostrarProdutos ? 0m : Valor;
        DespesaSelecionada.Data = DataDespesa;
        DespesaSelecionada.Tipo = TipoDespesaSelecionado;
        DespesaSelecionada.Notas = Notas;

        if (MostrarProdutos && ProdutoSelecionado != null)
        {
            DespesaSelecionada.ProductId = ProdutoSelecionado.ProductId;
            DespesaSelecionada.Quantidade = QuantidadeProduto;
        }

        _despesaRepository.AtualizarDespesa(DespesaSelecionada);
        EmEdicao = false;
        LimparFormulario();
        CarregarDespesas();
        AtualizarSaldo();
        MessageBox.Show("Despesa atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    public void CancelarEdicao()
    {
        EmEdicao = false;
        LimparFormulario();
    }

    [RelayCommand]
    public void DeletarDespesa(object? parameter)
    {
        var despesa = parameter as DespesaRepository.Despesa;
        if (despesa is null)
        {
            MessageBox.Show("Selecione uma despesa para deletar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var senhaDialog = new Adega_Oak.Views.PasswordDialog();
        if (senhaDialog.ShowDialog() != true)
            return;

        if (senhaDialog.Password != "ADEGA2024")
        {
            MessageBox.Show("Senha incorreta.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var resultado = MessageBox.Show($"Tem certeza que deseja excluir a despesa '{despesa.Descricao}'?\n\nEsta ação não poderá ser desfeita.", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (resultado != MessageBoxResult.Yes)
            return;

        _despesaRepository.DeletarDespesa(despesa.Id);
        LimparFormulario();
        CarregarDespesas();
        AtualizarSaldo();
        MessageBox.Show("Despesa excluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    public void MarcarComoPago(object? parameter)
    {
        var despesa = parameter as DespesaRepository.Despesa;
        if (despesa is null) return;
        despesa.Pago = !despesa.Pago;
        despesa.DataPagamento = despesa.Pago ? DateTime.Now : null;
        _despesaRepository.AtualizarDespesa(despesa);
        CarregarDespesas();
        AtualizarSaldo();
    }

    [RelayCommand]
    public void FiltrarPorPeriodo()
    {
        var dataInicio = new DateTime(AnoSelecionado, MesSelecionado, 1);
        var dataFim = dataInicio.AddMonths(1).AddDays(-1);
        var despesasCarregadas = _despesaRepository.CarregarDespesasPorPeriodo(dataInicio, dataFim);
        Despesas.Clear();
        foreach (var d in despesasCarregadas.OrderByDescending(x => x.Data)) Despesas.Add(d);
        AtualizarTotaisPorPeriodo(despesasCarregadas);
    }

    private void LimparFormulario()
    {
        Descricao = string.Empty;
        Valor = 0;
        DataDespesa = DateTime.Now;
        TipoDespesaSelecionado = DespesaRepository.TipoDespesa.Outros;
        Notas = string.Empty;
        DespesaSelecionada = null;
        EmEdicao = false;
        ProdutoSelecionado = null;
        QuantidadeProduto = 1;
        MostrarProdutos = false;
    }

    private void AtualizarTotais()
    {
        TotalDespesasNaoPagas = _despesaRepository.ObterTotalDespesasNaoPagas();
        TotalDespesasPagas = _despesaRepository.ObterTotalDespesasPagas();
    }

    private void AtualizarTotaisPorPeriodo(List<DespesaRepository.Despesa> despesas)
    {
        TotalDespesasNaoPagas = despesas.Where(d => !d.Pago).Sum(d => d.Valor);
        TotalDespesasPagas = despesas.Where(d => d.Pago).Sum(d => d.Valor);
    }

    private void AtualizarSaldo()
    {
        var saldo = _saldoRepository.ObterSaldoGeral();
        SaldoAtualizado = saldo.Saldo;
    }

    // Replace partial method implementation with private helper
    private void FiltrarProdutos(string? value)
    {
        string Normalizar(string texto) => string.Concat(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(value))
        {
            ProdutosFiltrados = new ObservableCollection<MainRepository.Produto>(Produtos);
            if (ProdutoSelecionado != null)
                ProdutoSelecionado = null;
            return;
        }

        var palavrasFiltro = Normalizar(value).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var filtrados = Produtos.Where(p =>
        {
            var busca = $"{p.Bebida} {p.Tamanho} {p.Material} {p.Descricao}";
            var buscaNorm = Normalizar(busca);
            return palavrasFiltro.All(palavra => buscaNorm.Contains(palavra));
        }).ToList();

        ProdutosFiltrados = new ObservableCollection<MainRepository.Produto>(filtrados);

        var correspondenciaExata = filtrados.FirstOrDefault(p => Normalizar(p.Descricao) == Normalizar(value));
        if (correspondenciaExata == null && ProdutoSelecionado != null)
            ProdutoSelecionado = null;
    }
}
