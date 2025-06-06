
using LearnQuestV1.Core.Enums;
namespace LearnQuestV1.Api.DTOs.Contents
{
    public class ContentSummaryDto
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public int ContentOrder { get; set; }
        public bool IsVisible { get; set; }
    }
}
