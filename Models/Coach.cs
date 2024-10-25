using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AnyoneForTennis.Areas.Identity.Data;

namespace AnyoneForTennis.Models;

public partial class Coach
{
    [Key]
    public int CoachId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Biography { get; set; }

    public string? Photo { get; set; }

    [ForeignKey("UserId")]
    public string UserId { get; set; } = null!;
    public virtual AnyoneForTennisUser User { get; set; } = null!;
}
