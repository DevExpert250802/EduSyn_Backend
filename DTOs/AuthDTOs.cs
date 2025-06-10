namespace edusync_backend.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
        public class RegisterDTO
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }


    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }
}
