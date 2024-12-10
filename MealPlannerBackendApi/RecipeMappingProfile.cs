using AutoMapper;
using MealPlannerBackend.Models;

namespace MealPlannerBackend
{
    public class RecipeMappingProfile : Profile
    {
        public RecipeMappingProfile()
        {
           
            CreateMap<KeyValuePair<string, RecipeDetails>, RecipeData>()
                .ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src => src.Value.Title))
                .ForMember(dest => dest.ImageURL, opt => opt.MapFrom(src => src.Value.Picture_link ?? "default_image_url.jpg"))
                .ForMember(dest => dest.Ingredients, opt => opt.MapFrom(src => src.Value.Ingredients.ToArray()))
                .ForMember(dest => dest.Instructions, opt => opt.MapFrom(src => src.Value.Instructions))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src=> src.Key));

        }
    }
}
