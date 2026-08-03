namespace Co_working_Space.Models.ViewModels;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public List<string> Roles { get; set; } = new();
}
