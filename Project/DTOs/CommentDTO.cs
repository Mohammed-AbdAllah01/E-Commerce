using System.ComponentModel.DataAnnotations;

namespace Project.DTOs
{
    public class CommentDTO
    {
        public int productId { get; set; }
        public  string customerId { get; set; }

        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5.")]
        public int Star { get; set; }
        public string OriginalComment { get; set; }
        public string TranslateComment { get; set; }
        public double CommentRate { get; set; }


    }
}
