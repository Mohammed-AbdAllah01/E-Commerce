using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Project.Enums;
using Project.Tables;
using System.ComponentModel.DataAnnotations;

namespace Project.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public double Discount { get; set; }
        public double Unite { get; set; }
        public double SellPrice { get; set; }
        public string CategoryName { get; set; }
        public string Image { get; set; }        
        public int Quantity { get; set; }
    }

        public class ProductDetailDTO
        {
        public int Id { get; set; }
        public string ProductName { get; set; }
            public string Color { get; set; }
            public string  Size { get; set; }
            public string Image { get; set; }
            public int Quantity { get; set; }
          }
    public class AllProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Feedback { get; set; }
        public string Status { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public double SellPrice { get; set; }
        public string MerchantName { get; set; }
        public string CategoryName { get; set; }
        public string MerchantId { get; set; }
        public string Image { get; set; }
    }

    public class AddFullProductDTO
        {
            [Required]
            public string Title { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string CategoryName { get; set; }

            public required string Type { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public double UnitPrice { get; set; }

            
            public double Discount { get; set; }

             [Required]
             public string merchantId { get; set; }

          

    }
    public class ProductModelDTO
    {
        public required int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }


    public class ColorSizeDTO
    {
        public int ProductId { get; set; }
        public string Color { get; set; }
        public string[] Size { get; set; }
         public int[] Quantity { get; set; }

        
        [FromForm]
        public IFormFile? image1 { get; set; }

        
        [FromForm]
        public IFormFile? image2 { get; set; }

        
        [FromForm]
        public IFormFile? image3 { get; set; }
    }




        public class AddProductCommentDTO
        {
            public int ProductId { get; set; }

            [Required]
            [Range(1, 5)]
            public int Feedback { get; set; }

            [MaxLength(500)]
            public string Comment { get; set; }

            //  public double? Feeling { get; set; } 
        }





}
