using Microsoft.AspNetCore.Identity;

namespace Co_working_Space.Models;

public class Wallet
{
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public IdentityUser User { get; set; } = null!;
}
