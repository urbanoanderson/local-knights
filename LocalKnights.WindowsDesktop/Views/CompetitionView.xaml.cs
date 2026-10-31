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

            if (comboBox.DataContext is not CompetitionPagePairingTableItem pairingTableItem ||
                !TryGetAddedResult(e, out string selectedResult))
            {
                return;
            }

            pairingTableItem.CommitSelectedResult(selectedResult);
        }

        internal static bool TryGetAddedResult(SelectionChangedEventArgs e, out string selectedResult)
        {
            selectedResult = string.Empty;
            if (e.AddedItems.Count != 1 || e.AddedItems[0] is not string result)
            {
                return false;
            }

            selectedResult = result;
            return true;
        }
    }
}