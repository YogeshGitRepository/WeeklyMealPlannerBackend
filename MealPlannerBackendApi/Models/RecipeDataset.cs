using System.Text.Json.Serialization;

namespace MealPlannerBackend.Models
{
    public class RecipeDataset
    {
        [JsonPropertyName("recipes")]
        public Dictionary<string, RecipeDetails> Recipes { get; set; }
    }
    public class RecipeDetails
    {
        [JsonPropertyName("ingredients")]
        public string[] Ingredients { get; set; }

        [JsonPropertyName("picture_link")]
        public string Picture_link { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }
    }
}
