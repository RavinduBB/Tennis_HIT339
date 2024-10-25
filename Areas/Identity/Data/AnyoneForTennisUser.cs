using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Identity;

namespace AnyoneForTennis.Areas.Identity.Data;

public class AnyoneForTennisUser : IdentityUser
{
    public virtual Member? Member { get; set; }
    public virtual Coach? Coach { get; set; }
}
