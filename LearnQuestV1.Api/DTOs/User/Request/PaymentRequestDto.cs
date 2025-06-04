namespace LearnQuestV1.Api.DTOs.Users.Request
{
    public class PaymentRequestDto
    {
        public int CourseId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}
