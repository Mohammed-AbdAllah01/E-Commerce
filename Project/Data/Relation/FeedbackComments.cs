using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class FeedbackComments
    {
        [ForeignKey("product")]
        public int feedbackId { get; set; }
        public required Feedback feedback { get; set; }

        public required string OriginalComment { get; set; }
        public required string TranslateComment { get; set; }

        public DateTime DateCreate { get; set; } = DateTime.Now;
        public required double CommentRate { get; set; }


    }
}
