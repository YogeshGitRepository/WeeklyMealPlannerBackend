namespace MealPlannerBackend.Models
{
    public class RecipeSearchRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 12;

        public List<string> Ingredients { get; set; }
        public string Role { get; set; }
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value; 
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value;
        }
    }

}
