using System.ComponentModel.DataAnnotations;

namespace LearnQuestV1.Api.DTOs.Contents
{
    public class ReorderContentDto
    {
        [Required]
        public int ContentId { get; set; }

        [Required]
        public int NewOrder { get; set; }
    }
}
