using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LocalKnights.WindowsDesktop.DisplayObjects;

namespace LocalKnights.WindowsDesktop.Views
{
    /// <summary>
    /// Interaction logic for CompetitionsView.xaml
    /// </summary>
    public partial class CompetitionsView : UserControl
    {
        private ViewModels.CompetitionsViewModel? _currentViewModel;

        public CompetitionsView()
        {
            InitializeComponent();
            DataContextChanged += CompetitionsView_DataContextChanged;
        }

        private void CompetitionsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _currentViewModel = DataContext as ViewModels.CompetitionsViewModel;
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
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.CompetitionIdLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.CreationDateLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.TitleLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.TypeLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.TimeFormatLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.RatedLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.StateLabel) ||
                e.PropertyName == nameof(ViewModels.CompetitionsViewModel.WinnerLabel))
            {
                UpdateColumnHeaders();
            }
        }

        private void UpdateColumnHeaders()
        {
            if (_currentViewModel != null && CompetitionsDataGrid != null)
            {
                var columns = CompetitionsDataGrid.Columns;
                if (columns.Count >= 8)
                {
                    columns[0].Header = _currentViewModel.CompetitionIdLabel;
                    columns[1].Header = _currentViewModel.CreationDateLabel;
                    columns[2].Header = _currentViewModel.TitleLabel;
                    columns[3].Header = _currentViewModel.TypeLabel;
                    columns[4].Header = _currentViewModel.TimeFormatLabel;
                    columns[5].Header = _currentViewModel.RatedLabel;
                    columns[6].Header = _currentViewModel.StateLabel;
                    columns[7].Header = _currentViewModel.WinnerLabel;
                }
            }
        }

        private void ApplyInitialSortState()
        {
            if (_currentViewModel != null && CompetitionsDataGrid != null)
            {
                var sortParam = _currentViewModel.CurrentSortParameter;
                var column = GetColumnBySortProperty(sortParam.SortColumn);
                if (column != null)
                {
                    column.SortDirection = sortParam.SortDirection;
                }
            }
        }

        private DataGridColumn? GetColumnBySortProperty(string sortColumn)
        {
            string[] columnProperties = new[]
            {
                nameof(Common.Models.Competition.Id),              // 0
                nameof(Common.Models.Competition.CreatedAt),       // 1
                nameof(Common.Models.Competition.Title),           // 2
                nameof(Common.Models.Competition.Type),            // 3
                nameof(Common.Models.Competition.TimeFormat),      // 4
                nameof(Common.Models.Competition.IsRated),         // 5
                nameof(Common.Models.Competition.State),           // 6
                nameof(Common.Models.Competition.WinnerId)           // 7
            };

            for (int i = 0; i < columnProperties.Length; i++)
            {
                if (columnProperties[i] == sortColumn && i < CompetitionsDataGrid.Columns.Count)
                {
                    return CompetitionsDataGrid.Columns[i];
                }
            }

            return null;
        }

        private void CompetitionsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Prevent default WPF sorting
            e.Handled = true;

            // Get the property name based on column index
            string[] columnProperties = new[]
            {
                nameof(Common.Models.Competition.Id),              // 0
                nameof(Common.Models.Competition.CreatedAt),       // 1
                nameof(Common.Models.Competition.Title),           // 2
                nameof(Common.Models.Competition.Type),            // 3
                nameof(Common.Models.Competition.TimeFormat),      // 4
                nameof(Common.Models.Competition.IsRated),         // 5
                nameof(Common.Models.Competition.State),           // 6
                nameof(Common.Models.Competition.WinnerId)           // 7
            };

            int columnIndex = CompetitionsDataGrid.Columns.IndexOf(e.Column);
            string propertyName = columnIndex >= 0 && columnIndex < columnProperties.Length
                ? columnProperties[columnIndex]
                : nameof(Common.Models.Competition.CreatedAt);

            // Call the ViewModel's ChangeSortCommand and update visual sort indicators
            var viewModel = DataContext as ViewModels.CompetitionsViewModel;
            if (viewModel != null)
            {
                viewModel.ChangeSortCommand.Execute(propertyName);

                // Clear other column indicators
                foreach (var col in CompetitionsDataGrid.Columns)
                {
                    col.SortDirection = null;
                }

                // Set the sort indicator on the clicked column
                e.Column.SortDirection = viewModel.CurrentSortParameter.SortDirection;
            }
        }

        private void CompetitionsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CompetitionsDataGrid.SelectedItem is CompetitionsPageTableItem competition)
            {
                var viewModel = DataContext as ViewModels.CompetitionsViewModel;
                if (viewModel != null)
                {
                    viewModel.ViewCompetitionCommand.Execute(competition.Id);
                }
            }
        }
    }
}
