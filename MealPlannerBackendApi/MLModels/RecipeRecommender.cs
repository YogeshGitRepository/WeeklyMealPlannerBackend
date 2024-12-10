using MealPlannerBackend.Models;
using Microsoft.ML;
using AutoMapper;
using System.Text.Json;

namespace MealPlannerBackend.MLModels
{
    public class RecipeRecommender
    {
        private static string _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "recipes_raw_nosource_epi.json");
        private static string _idvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "recipes_data.idv");
        private static string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "recipe_recommender_model.zip");
        private static MLContext mlContext = new MLContext();

       
        public static void ConvertJsonToIdv()
        {
         
            var dataView = LoadJsonData();

           
            using (var fileStream = new FileStream(_idvPath, FileMode.Create, FileAccess.Write))
            {
                mlContext.Data.SaveAsBinary(dataView, fileStream);
            }

            Console.WriteLine("Data has been successfully converted to .idv format!");
        }

      
        public static IDataView LoadData()
        {
            if (File.Exists(_idvPath))
            {
               
                Console.WriteLine("Loading data from .idv file.");
                return mlContext.Data.LoadFromBinary(_idvPath);
            }
            else
            {
               
                Console.WriteLine(".idv file not found, loading from JSON and converting to .idv.");
                ConvertJsonToIdv();
                return LoadData();
            }
        }

      
        private static IDataView LoadJsonData()
        {
          
            if (!File.Exists(_dataPath))
                throw new FileNotFoundException($"The JSON data file at {_dataPath} was not found.");

            var jsonData = File.ReadAllText(_dataPath);

            if (string.IsNullOrWhiteSpace(jsonData))
                throw new ArgumentException("The JSON data is empty.");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

           
            var recipesDict = JsonSerializer.Deserialize<Dictionary<string, RecipeDetails>>(jsonData, options);            
            if (recipesDict == null)
            {
                throw new ArgumentNullException(nameof(recipesDict), "Deserialized recipe data is null. Please check JSON structure.");
            }

          
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RecipeMappingProfile>());
            var mapper = config.CreateMapper();

         
            var recipeDataList = recipesDict
                .Select(kvp => mapper.Map<RecipeData>(kvp))
                .ToList();

            return mlContext.Data.LoadFromEnumerable(recipeDataList);
        }

        public static void TrainAndSaveModel()
        {
            var data = LoadData();

          
            var splitData = mlContext.Data.TrainTestSplit(data, testFraction: 0.8);
            var trainData = mlContext.Data.Cache(splitData.TrainSet);
            var sampleData = mlContext.Data.TakeRows(trainData, 100);  

           
            var pipeline = mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "IngredientsFeaturized",
                    inputColumnName: nameof(RecipeData.Ingredients))
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(RecipeData.RecipeName)))
                .Append(mlContext.Transforms.Concatenate("Features", "IngredientsFeaturized"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(
                    labelColumnName: "Label",
                    featureColumnName: "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var model = pipeline.Fit(sampleData);
            stopwatch.Stop();
            Console.WriteLine($"Model training time: {stopwatch.Elapsed.TotalMinutes} minutes");

           
            mlContext.Model.Save(model, data.Schema, _modelPath);
        }

        public static List<RecipeData> PredictRecipe(string[] ingredients)
        {
            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException("Model file not found. Please train the model first.");
            }

          
            var allRecipes = LoadData(); 

           
            var recipesList = mlContext.Data.CreateEnumerable<RecipeData>(allRecipes, reuseRowObject: false).ToList();

          
            var matchingRecipes = recipesList
 .Where(recipe => recipe.Ingredients != null &&
                     ingredients != null &&
                     ingredients.All(ingredient => recipe.Ingredients
                         .Any(c => c != null && c.Contains(ingredient, StringComparison.OrdinalIgnoreCase)))).ToList();

            if (!matchingRecipes.Any())
            {
                throw new ArgumentException("No recipes found matching the provided ingredients.");
            }

          
            return matchingRecipes;
        }

    }
}