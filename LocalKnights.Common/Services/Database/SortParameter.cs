using System.ComponentModel;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Encapsulates sorting parameters for database queries.
    /// </summary>
    public class SortParameter
    {
        /// <summary>
        /// Gets or sets the column name to sort by.
        /// </summary>
        public string SortColumn { get; set; } = "Id";

        /// <summary>
        /// Gets or sets the sort direction (ascending or descending).
        /// </summary>
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

        /// <summary>
        /// Initializes a new instance of the SortParameter class.
        /// </summary>
        /// <param name="sortColumn">The column name to sort by.</param>
        /// <param name="sortDirection">The sort direction.</param>
        public SortParameter(string sortColumn, ListSortDirection sortDirection)
        {
            SortColumn = sortColumn;
            SortDirection = sortDirection;
        }

        /// <summary>
        /// Creates a default sort parameter (Id ascending).
        /// </summary>
        public static SortParameter Default() => new("Id", ListSortDirection.Ascending);
    }
}
