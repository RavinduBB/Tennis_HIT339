using System.ComponentModel.DataAnnotations.Schema;
using AnyoneForTennis.Areas.Identity.Data;

namespace AnyoneForTennis.Models;

public class Member
{
    public int MemberId { get; set; }
    public string? FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public bool Active { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }
    public virtual AnyoneForTennisUser User { get; set; }
    public virtual ICollection<MemberSchedule> MemberSchedules { get; set; } = new List<MemberSchedule>();
}
