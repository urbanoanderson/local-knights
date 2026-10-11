using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LocalKnights.WindowsDesktop.Views
{
    public partial class GamesView : UserControl
    {
        private ViewModels.GamesViewModel? _currentViewModel;

        public GamesView()
        {
            InitializeComponent();
            DataContextChanged += GamesView_DataContextChanged;
        }

        private void GamesView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _currentViewModel = DataContext as ViewModels.GamesViewModel;
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdateColumnHeaders();
                ApplyInitialSortState();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LanguageChanged" ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.GameIdLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.CreationDateLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.ParticipantsLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.ResultLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.TimeFormatLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.RatedLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.RatingChangeLabel) ||
                e.PropertyName == nameof(ViewModels.GamesViewModel.CompetitionIdLabel))
            {
                UpdateColumnHeaders();
            }
            else if (e.PropertyName == nameof(ViewModels.GamesViewModel.Games))
            {
                ApplyInitialSortState();
            }
        }

        private void UpdateColumnHeaders()
        {
            if (_currentViewModel != null && GamesDataGrid != null)
            {
                var columns = GamesDataGrid.Columns;
                if (columns.Count >= 8)
                {
                    columns[0].Header = _currentViewModel.GameIdLabel;
                    columns[1].Header = _currentViewModel.CreationDateLabel;
                    columns[2].Header = _currentViewModel.ParticipantsLabel;
                    columns[3].Header = _currentViewModel.ResultLabel;
                    columns[4].Header = _currentViewModel.TimeFormatLabel;
                    columns[5].Header = _currentViewModel.RatedLabel;
                    columns[6].Header = _currentViewModel.RatingChangeLabel;
                    columns[7].Header = _currentViewModel.CompetitionIdLabel;
                }
            }
        }

        private void ApplyInitialSortState()
        {
            if (_currentViewModel != null && GamesDataGrid != null)
            {
                foreach (DataGridColumn column in GamesDataGrid.Columns)
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
                nameof(Common.Models.Game.Id),                      // 0
                nameof(Common.Models.Game.CreatedAt),               // 1
                nameof(Common.Models.Game.WhitePlayerId),           // 2
                nameof(Common.Models.Game.Result),                  // 3
                nameof(Common.Models.Game.TimeFormat),              // 4
                nameof(Common.Models.Game.IsRated),                 // 5
                nameof(Common.Models.Game.WhitePlayerRatingChange), // 6
                nameof(Common.Models.Game.Id)                       // 7
            };

            int index = Array.IndexOf(columnProperties, sortColumn);
            return index >= 0 && index < GamesDataGrid.Columns.Count
                ? GamesDataGrid.Columns[index]
                : null;
        }

        private void GamesDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Prevent default WPF sorting
            e.Handled = true;

            // Get the property name based on column index
            string[] columnProperties = new[]
            {
                nameof(Common.Models.Game.Id),                      // 0
                nameof(Common.Models.Game.CreatedAt),               // 1
                nameof(Common.Models.Game.WhitePlayerId),           // 2 (Participants)
                nameof(Common.Models.Game.Result),                  // 3
                nameof(Common.Models.Game.TimeFormat),              // 4
                nameof(Common.Models.Game.IsRated),                 // 5 (Rated)
                nameof(Common.Models.Game.WhitePlayerRatingChange), // 6 (RatingChange)
                nameof(Common.Models.Game.Id)            // 7 (Competition id not a direct Game property)
            };

            int columnIndex = GamesDataGrid.Columns.IndexOf(e.Column);
            string propertyName = columnIndex >= 0 && columnIndex < columnProperties.Length
                ? columnProperties[columnIndex]
                : nameof(Common.Models.Game.CreatedAt);

            // Call the ViewModel's ChangeSortCommand and update visual sort indicators
            var viewModel = DataContext as ViewModels.GamesViewModel;
            if (viewModel != null)
            {
                viewModel.ChangeSortCommand.Execute(propertyName);

                // Clear other column indicators
                foreach (var col in GamesDataGrid.Columns)
                {
                    col.SortDirection = null;
                }

                // Set the sort indicator on the clicked column
                e.Column.SortDirection = viewModel.CurrentSortParameter.SortDirection;
            }
        }

        private void GamesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row?.DataContext is DisplayObjects.GamesPageTableItem game)
            {
                var command = (DataContext as ViewModels.GamesViewModel)?.SelectGameCommand;
                command?.Execute(game);
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
