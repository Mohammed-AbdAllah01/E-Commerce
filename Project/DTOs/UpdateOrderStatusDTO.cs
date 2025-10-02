namespace Project.DTOs
{
    public class UpdateOrderStatusDTO
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; }

    }

    public class UpdateOrderItemStatusDTO
    {
        public int Id { get; set; }
       public string NewStatus { get; set; }



    }
}
