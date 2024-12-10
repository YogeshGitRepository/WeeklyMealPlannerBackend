namespace MealPlannerBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int FamilySize { get; set; }
        public string SecretQuestion { get; set; } 
        public string Answer { get; set; } 
    }


    public class Login
    {
        public string Email { get; set; } 
        public string Password { get; set; } 
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string SecretQuestion { get; set; }
        public string Answer { get; set; }
    }
}
