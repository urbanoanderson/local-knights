using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LocalKnights.WindowsDesktop.Views
{
    public partial class PlayersView : UserControl
    {
        private ViewModels.PlayersViewModel? _currentViewModel;

        public PlayersView()
        {
            InitializeComponent();
            DataContextChanged += PlayersView_DataContextChanged;
        }

        private void PlayersView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from previous ViewModel
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Subscribe to new ViewModel
            _currentViewModel = DataContext as ViewModels.PlayersViewModel;
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdateColumnHeaders();
                ApplyInitialSortState();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Update headers when language changes
            if (e.PropertyName == "LanguageChanged" ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.IdentifierNameLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.DisplayNameLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.ChessTitleLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.CreationDateLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.BulletRatingLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.BlitzRatingLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.RapidRatingLabel) ||
                e.PropertyName == nameof(ViewModels.PlayersViewModel.ClassicalRatingLabel))
            {
                UpdateColumnHeaders();
            }
            else if (e.PropertyName == nameof(ViewModels.PlayersViewModel.Players))
            {
                ApplyInitialSortState();
            }
        }

        private void UpdateColumnHeaders()
        {
            if (_currentViewModel != null && PlayersDataGrid != null)
            {
                var columns = PlayersDataGrid.Columns;
                // There are 9 columns in the Players DataGrid (including the Age template column).
                if (columns.Count >= 9)
                {
                    columns[0].Header = _currentViewModel.IdentifierNameLabel;
                    columns[1].Header = _currentViewModel.DisplayNameLabel;
                    columns[2].Header = _currentViewModel.ChessTitleLabel;
                    // columns[3] is the Age template column and keeps its static header from XAML
                    columns[4].Header = _currentViewModel.CreationDateLabel;
                    columns[5].Header = _currentViewModel.BulletRatingLabel;
                    columns[6].Header = _currentViewModel.BlitzRatingLabel;
                    columns[7].Header = _currentViewModel.RapidRatingLabel;
                    columns[8].Header = _currentViewModel.ClassicalRatingLabel;
                }
            }
        }

        private void ApplyInitialSortState()
        {
            if (_currentViewModel != null && PlayersDataGrid != null)
            {
                foreach (DataGridColumn column in PlayersDataGrid.Columns)
                {
                    column.SortDirection = null;
                }

                var sortParam = _currentViewModel.CurrentSortParameter;
                var sortedColumn = GetColumnBySortProperty(sortParam.SortColumn);
                if (sortedColumn != null)
                {
                    sortedColumn.SortDirection = sortParam.SortDirection;
                }
            }
        }

        private DataGridColumn? GetColumnBySortProperty(string sortColumn)
        {
            string[] columnProperties = new[]
            {
                nameof(Common.Models.Player.IdentifierName),    // 0
                nameof(Common.Models.Player.DisplayName),       // 1
                nameof(Common.Models.Player.ChessTitle),        // 2
                nameof(Common.Models.Player.BirthDate),         // 3
                nameof(Common.Models.Player.CreatedAt),         // 4
                nameof(Common.Models.Player.BulletRating),      // 5
                nameof(Common.Models.Player.BlitzRating),       // 6
                nameof(Common.Models.Player.RapidRating),       // 7
                nameof(Common.Models.Player.ClassicalRating)    // 8
            };

            int index = Array.IndexOf(columnProperties, sortColumn);
            return index >= 0 && index < PlayersDataGrid.Columns.Count
                ? PlayersDataGrid.Columns[index]
                : null;
        }

        private void PlayersDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Prevent default WPF sorting
            e.Handled = true;

            // Get the property name based on column index
            string[] columnProperties = new[]
            {
                nameof(Common.Models.Player.IdentifierName),    // 0
                nameof(Common.Models.Player.DisplayName),       // 1
                nameof(Common.Models.Player.ChessTitle),        // 2
                nameof(Common.Models.Player.BirthDate),         // 3 (Age template column)
                nameof(Common.Models.Player.CreatedAt),         // 4
                nameof(Common.Models.Player.BulletRating),      // 5
                nameof(Common.Models.Player.BlitzRating),       // 6
                nameof(Common.Models.Player.RapidRating),       // 7
                nameof(Common.Models.Player.ClassicalRating)    // 8
            };

            int columnIndex = PlayersDataGrid.Columns.IndexOf(e.Column);
            string propertyName = columnIndex >= 0 && columnIndex < columnProperties.Length
                ? columnProperties[columnIndex]
                : nameof(Common.Models.Player.IdentifierName);

            // Call the ViewModel's ChangeSortCommand and update visual sort indicators
            var viewModel = DataContext as ViewModels.PlayersViewModel;
            if (viewModel != null)
            {
                viewModel.ChangeSortCommand.Execute(propertyName);

                // Clear other column indicators
                foreach (var col in PlayersDataGrid.Columns)
                {
                    col.SortDirection = null;
                }

                // Set the sort indicator on the clicked column
                e.Column.SortDirection = viewModel.CurrentSortParameter.SortDirection;
            }
        }

        private void PlayersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row?.DataContext is DisplayObjects.PlayersPageTableItem player)
            {
                var command = (DataContext as ViewModels.PlayersViewModel)?.SelectPlayerCommand;
                command?.Execute(player);
            }
        }

        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                {
                    return parent;
                }

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }
    }
}
