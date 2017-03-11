using System;

namespace FinalProject.Model
{
    public class Calendar : IModel
    {
        public int Id { get; set; }

        public string FilePath { get; set; }

        public virtual Student Student { get; set; }

        public TimeSpan MaxScheduleTime { get; set; }
    }
}