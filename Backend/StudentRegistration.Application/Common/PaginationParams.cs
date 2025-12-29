using System.ComponentModel.DataAnnotations;

namespace StudentRegistration.Application.Common
{
    /// <summary>
    /// Standard pagination parameters for API requests
    /// </summary>
    public class PaginationParams
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 100 ? 100 : (value < 1 ? 10 : value);
        }
    }
}
