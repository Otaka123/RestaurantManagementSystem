namespace UsersApp.ViewModels
{
    public class EditProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // لتخزين الصورة الجديدة
        public IFormFile? ProfilePicture { get; set; }
    }
}
