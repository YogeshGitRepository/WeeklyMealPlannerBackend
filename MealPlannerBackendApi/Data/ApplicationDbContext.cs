using MealPlannerBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RecipeData> Recipes { get; set; }
        public DbSet<FamilySize> FamilySizes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipePrediction> RecipePredictions { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<RecipeName> RecipeNames { get; set; }
        public DbSet<SearchIngredient> SearchIngredients { get; set; }
        public DbSet<WeeklyCalendar> WeeklyCalendars { get; set; }
        public DbSet<Recipe> Recipe { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>()
             .HasKey(r => r.Id);

            modelBuilder.Entity<WeeklyCalendar>()
                .HasKey(wc => wc.Id);

            
            modelBuilder.Entity<WeeklyCalendar>()
                .HasOne(wc => wc.Recipe)
                .WithMany(r => r.WeeklyCalendars)
                .HasForeignKey(wc => wc.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<WeeklyCalendar>()
                .HasIndex(wc => new { wc.DayOfWeek, wc.SlotId })
                .IsUnique();

           
            modelBuilder.Entity<RecipePrediction>()
                .HasNoKey();

            modelBuilder.Entity<SearchIngredient>()
               .HasOne(si => si.SearchLog)
               .WithMany(sl => sl.SearchIngredients)
               .HasForeignKey(si => si.SearchLogId)
               .OnDelete(DeleteBehavior.Cascade);

         
            modelBuilder.Entity<RecipeName>()
                .HasOne(r => r.SearchLog)
                .WithMany(s => s.RecipeNames)
                .HasForeignKey(r => r.SearchLogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 4); 
            modelBuilder.Entity<SearchIngredient>().Property(si => si.Role)
           .HasMaxLength(50)
           .IsRequired();
        }
    }
}
