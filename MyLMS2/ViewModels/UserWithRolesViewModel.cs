namespace MyLMS2.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
