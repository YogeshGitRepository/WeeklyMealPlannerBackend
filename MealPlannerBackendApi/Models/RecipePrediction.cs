using Microsoft.ML.Data;

namespace MealPlannerBackend.Models
{
    public class RecipePrediction
    {

        [ColumnName("PredictedLabel")]
        public string RecipeName { get; set; }
    }
}
