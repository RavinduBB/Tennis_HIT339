using AnyoneForTennis.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnyoneForTennis.Models;

public partial class Schedule
{
    [Key]
    public int ScheduleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public string? Description { get; set; }

    [ForeignKey("Coach")]
    public int CoachId { get; set; }

    public Coach? Coach { get; set; }

    public ICollection<MemberSchedule> MemberSchedules { get; set; } = [];
}
