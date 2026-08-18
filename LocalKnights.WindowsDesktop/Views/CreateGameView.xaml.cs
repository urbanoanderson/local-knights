using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

namespace LocalKnights.WindowsDesktop.Views
{
    public partial class CreateGameView : UserControl
    {
        public CreateGameView()
        {
            InitializeComponent();
        }

        private void WhitePlayerSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateGameViewModel vm)
            {
                vm.SetWhiteSearchFocus(true);
            }
        }

        private void WhitePlayerSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is ViewModels.CreateGameViewModel vm)
                {
                    var keepOpen = WhitePlayerSearchBox.IsKeyboardFocusWithin ||
                                   WhiteSuggestionsContainer.IsKeyboardFocusWithin ||
                                   WhiteSuggestionsContainer.IsMouseOver;
                    vm.SetWhiteSearchFocus(keepOpen);
                }
            }), DispatcherPriority.Background);
        }

        private void BlackPlayerSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateGameViewModel vm)
            {
                vm.SetBlackSearchFocus(true);
            }
        }

        private void BlackPlayerSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is ViewModels.CreateGameViewModel vm)
                {
                    var keepOpen = BlackPlayerSearchBox.IsKeyboardFocusWithin ||
                                   BlackSuggestionsContainer.IsKeyboardFocusWithin ||
                                   BlackSuggestionsContainer.IsMouseOver;
                    vm.SetBlackSearchFocus(keepOpen);
                }
            }), DispatcherPriority.Background);
        }
    }
}
