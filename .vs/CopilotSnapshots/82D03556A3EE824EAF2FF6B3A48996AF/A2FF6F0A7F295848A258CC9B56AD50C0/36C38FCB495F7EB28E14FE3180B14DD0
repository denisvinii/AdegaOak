using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Adega_Oak.Features.Estoque;

public partial class EstoqueViewModel : ObservableObject
{
    private readonly EstoqueRepository _estoqueRepository;

    [ObservableProperty]
    private ObservableCollection<EstoqueRepository.EstoqueView> estoque = new();

    public EstoqueViewModel(EstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
        CarregarEstoque();
    }

    [RelayCommand]
    public void CarregarEstoque()
    {
        var estoqueAtualizado = _estoqueRepository.CarregarEstoqueCompleto();
        Estoque.Clear();
        foreach (var item in estoqueAtualizado)
        {
            Estoque.Add(item);
        }
    }

    // Adicionando o comando AtualizarCommand
    public IRelayCommand AtualizarCommand => new RelayCommand(CarregarEstoque);
}