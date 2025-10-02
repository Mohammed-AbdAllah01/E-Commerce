using Project.Enums;

namespace Project.DTOs
{
    public class specificOrderDto
    {
        public required int Id { get; set; }
        public required string status { get; set; }
        public required string CustomerId { get; set; }
        public required string UserName { get; set; }

        
        public required string address { get; set; }
        public required string phone { get; set; }

        public required int[] ProductId { get; set; }
        public required string[] ProductsName { get; set; }
        public required double TotalPrice { get; set; }

        public required int[] quantity { get; set; }
        public required string[] color { get; set; }
        public required string[] size { get; set; }
        public required double[] unitprice { get; set; }
        public required string[] image { get; set; }

    }
    public class specificOrderadminDto
    {
        public required int Id { get; set; }
                public required string[] status { get; set; }
        public required string CustomerId { get; set; }
        public required string UserName { get; set; }

        
        public required string address { get; set; }
        public required string phone { get; set; }

        public required int[] ProductId { get; set; }
        public required string[] ProductsName { get; set; }
        public required double TotalPrice { get; set; }
        public required int[] quantity { get; set; }
        public required string DeliveryId { get; set; }
        public required string DeliveryName { get; set; }



    }
    public class specificOrdercusDto
    {
        public required int OrderId { get; set; }
        public required string[] status { get; set; }        
        public required string UserName { get; set; }
        public required string address { get; set; }
        public required string phone { get; set; }
        public required int[] ProductId { get; set; }
        public required string[] ProductsName { get; set; }
        public required string[] color { get; set; }
        public required string[] size { get; set; }
        public required double[] unitprice { get; set; }
        public required double TotalPrice { get; set; }
        public required int[] quantity { get; set; }
        public required string DeliveryName { get; set; }




    }
}
