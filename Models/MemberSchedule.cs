using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnyoneForTennis.Models
{
    public class MemberSchedule
    {
        [Key]
        public int MemberScheduleId { get; set; }
        [ForeignKey("Member")]
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        [ForeignKey("Schedule")]
        public int ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
    }
}
