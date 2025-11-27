using BenchmarkDotNet.Attributes;
using Adega_Oak.Features.MainWindow;
using Adega_Oak.Repositories;
using Adega_Oak.Services;
using Adega_Oak.Features.Estoque;
using Adega_Oak.Features.Historico;
using Adega_Oak.Features.TabelaPrecos;
using Microsoft.VSDiagnostics;

namespace Adega_Oak.Benchmarks
{
    [CPUUsageDiagnoser]
    public class MainWindowViewModelInitializationBenchmark
    {
        [Benchmark]
        public void InitializeMainWindowViewModel()
        {
            var dbService = new DatabaseService();
            var mainRepository = new MainRepository(dbService);
            var estoqueRepository = new EstoqueRepository(dbService);
            var historicoRepository = new HistoricoRepository(dbService);
            var saldoRepository = new SaldoRepository(dbService);
            var estoqueViewModel = new EstoqueViewModel(estoqueRepository, mainRepository);
            var historicoViewModel = new HistoricoViewModel(historicoRepository);
            var tabelaPrecosViewModel = new TabelaPrecosViewModel(estoqueRepository);
            var mainWindowViewModel = new MainWindowViewModel(mainRepository, estoqueViewModel, historicoViewModel, saldoRepository, dbService, tabelaPrecosViewModel);
        }
    }
}