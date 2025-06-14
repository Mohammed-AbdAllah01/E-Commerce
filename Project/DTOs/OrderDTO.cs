using Project.Enums;

namespace Project.DTOs
{
    public class OrderDTO
    {
        public required int Id { get; set; }
        public required OrdStatus status { get; set; }
        public required string CustomerId { get; set; }
        public required string UserName { get; set; }

        public required string address { get; set; }
        
        public required string phone { get; set; }

        public required double TotalPrice { get; set; }



    }
    public class OrderadminDTO
    {
        public required int Id { get; set; }
        public required string status { get; set; }
        public required string CustomerId { get; set; }
        public required string UserName { get; set; }

       

        public required string address { get; set; }
        public required string phone { get; set; }

        public required string DeliveryId { get; set; }
        public required string DeliveryName { get; set; }

    }
    public class OrdercusDTO
    {
        public required int OrderId { get; set; }
        public required string status { get; set; }        
        public required string UserName { get; set; }
        public required string address { get; set; }
        public required string phone { get; set; }        
        public required string DeliveryName { get; set; }

    }


    public class AddOrderDTO
    {
        public string CustomerId { get; set; }       
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
