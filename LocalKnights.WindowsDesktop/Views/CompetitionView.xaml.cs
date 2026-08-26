using System.Windows.Controls;
using LocalKnights.WindowsDesktop.DisplayObjects;

namespace LocalKnights.WindowsDesktop.Views
{
    /// <summary>
    /// Interaction logic for CompetitionView.xaml
    /// </summary>
    public partial class CompetitionView : UserControl
    {
        public CompetitionView()
        {
            InitializeComponent();
        }

        private void PairingResultComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox || !comboBox.IsLoaded)
            {
                return;
            }

            if (comboBox.DataContext is not CompetitionPagePairingTableItem pairingTableItem)
            {
                return;
            }

            pairingTableItem.CommitSelectedResult();
        }
    }
}