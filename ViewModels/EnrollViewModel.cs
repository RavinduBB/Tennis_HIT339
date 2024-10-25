using AnyoneForTennis.Models;

public class EnrollViewModel
{
    public IEnumerable<Schedule> Schedules { get; set; }
    public List<int> EnrolledScheduleIds { get; set; }
}
