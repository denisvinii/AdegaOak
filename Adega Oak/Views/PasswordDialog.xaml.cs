using System.Windows;

namespace Adega_Oak.Views
{
    public partial class PasswordDialog : Window
    {
        #region Construtor
        public PasswordDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Propriedades
        // Retorna a senha informada pelo usuário
        public string Password => PasswordBox.Password;
        #endregion

        #region Eventos
        // Evento do botão OK
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Evento do botão Cancelar
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion
    }
}
