namespace edusync_backend.DTOs
{
    public class UserReadDTO
    {
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }

    public class UserCreateDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; } // Plain password input
    }

    public class UserUpdateDTO
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
