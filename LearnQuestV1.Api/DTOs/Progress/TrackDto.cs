﻿namespace LearnQuestV1.Api.DTOs.Progress
{
    public class TrackDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string TrackDescription { get; set; } = string.Empty;
        public int CourseCount { get; set; }
    }
}
