
using Microsoft.VisualBasic;
using Project.Enums;

namespace Project.DTOs
{
    public class dtoProductHome
    {
        public required int Id { get; set; }
        public required   double Discount { get; set; }
        public required double UnitPrice { get; set; }       
        public required double SellPrice { get; set; }
        public required double Feedback { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
        public required string Status { get; set; }

        public required string ImageUrl { get; set; }

    }

    public class dtoSpecificProduct
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Status { get; set; }
        public required double personStar { get; set; } // for individual user rating
        public required double averageStar { get; set; } // for average rating from users
        public required string[] OriginalComment { get; set; }
        public required int[] AllStars { get; set; } // for users rating
        public required string[] TranslateComment { get; set; }
        public DateTime[] DateCreate { get; set; }
        public required string[] UserName { get; set; }
        public required double UnitPrice { get; set; }
        public required double Discount { get; set; }
        public required double SellPrice { get; set; }
        public required string Description { get; set; }
        public required string[] Color { get; set; }
        public required int[] ColorId { get; set; }
        public required int CategoriesId { get; set; }
        public required string Type { get; set; }
        public required string Category { get; set; }
        public required double[] CommentRate { get; set; } 
        public required string MerchantName { get; set; }
        public required string MerchantId { get; set; }
        public required double MerchantFeedbak { get; set; }
        public List<string> ImageUrls { get; set; } // Changed from string[] to List<string>
        

    }


    public class FavProductDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public required string Status { get; set; }
        public string Title { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public double SellPrice { get; set; }
        public string Image { get; set; }
    }

    public class FavMerchantDTO
    {
        public int Id { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
        public string MerchantName { get; set; }        
        public string Image { get; set; }

        public double Rate { get; set; }




    }

    public class CartDTO
    {
        public int[] Id { get; set; }
       
       public required int[] ProductIds { get; set; }
        public required string[] ProductsNames { get; set; }
        public required string[] ProductDescribtions { get; set; }
        public required double[] ProductPrice { get; set; }
        public required int[] quantity { get; set; }
        public required string[] color { get; set; }
        public required string[] size { get; set; }
        public required string[] image { get; set; }

        public required double TotalPrice { get; set; }


    }

    public class AddCartDTO
    {

        public required int ProductId { get; set; }   
        public required int colorId { get; set; }
        public required int sizeId { get; set; }
        public string CustomerId { get; set; }
    }


    public class EditProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
 
    }

}
