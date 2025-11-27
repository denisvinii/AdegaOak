using Adega_Oak.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Adega_Oak.Features.Estoque;

public partial class EstoqueViewModel : ObservableObject
{
    private readonly EstoqueRepository _estoqueRepository;
    private readonly MainRepository _mainRepository;

    [ObservableProperty]
    private ObservableCollection<EstoqueRepository.EstoqueView> estoque = new();

    public EstoqueViewModel(EstoqueRepository estoqueRepository, MainRepository mainRepository)
    {
        _estoqueRepository = estoqueRepository;
        _mainRepository = mainRepository;
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

    [RelayCommand]
    public void ExcluirProduto(EstoqueRepository.EstoqueView produto)
    {
        if (produto.Quantidade != 0)
        {
            System.Windows.MessageBox.Show("Só é possível excluir produtos com quantidade igual a 0.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        // Solicita senha com PasswordBox
        var dialog = new Adega_Oak.Views.PasswordDialog();
        var result = dialog.ShowDialog();
        if (result != true)
            return;
        var senha = dialog.Password;
        if (senha != "ADEGA2024")
        {
            System.Windows.MessageBox.Show("Senha incorreta.", "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }
        _mainRepository.ExcluirProduto(produto.ProductId);
        CarregarEstoque();
        System.Windows.MessageBox.Show("Produto excluído com sucesso.", "Sucesso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

}