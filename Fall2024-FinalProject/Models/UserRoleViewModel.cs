namespace Fall2024_FinalProject.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CurrentRole { get; set; }
        public List<string> AllRoles { get; set; }
        public string SelectedRole { get; set; } 
    }
}
