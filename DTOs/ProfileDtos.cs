namespace LearningAppNetCoreApi.Dtos
{
    public class ProfileStatsDto
    {
        public int PathsStarted { get; set; }
        public int PathsInProgress { get; set; }
        public int PathsCompleted { get; set; }
        public int ItemsCompleted { get; set; }
        public int QuizzesCompleted { get; set; }
        public DateTime JoinedDate { get; set; }
    }
}
