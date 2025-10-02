using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Enums;
using Project.Services.Interfaces;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryPersonController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public DeliveryPersonController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


    
        //  Get All Active Orders for Delivery Person
        [HttpGet("GetAssignedOrders")]
        [Authorize(Roles = "DeliveryRep")]
        public async Task<IActionResult> GetAssignedOrders(string deliveryPersonId)
        {
            //  Check if the delivery person exists
            if (string.IsNullOrEmpty(deliveryPersonId))
                return BadRequest(new { message = "Delivery person ID is required." });

            var deliveryPerson = await _context.Users.FindAsync(deliveryPersonId);
            if (deliveryPerson == null)
                return NotFound(new { message = "Delivery person not found." });

            var orders = await _context.Orders
                .Where(o => o.DeliveryId == deliveryPersonId)
                .Include(o => o.customer)
                .Include(o => o.orderItems)
                .ToListAsync();
            if (orders == null || orders.Count == 0)
                return BadRequest(new { message = "No orders found for this delivery person." });
            var orderDTOs = orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                status = o.Status,
                CustomerId = o.CustomerId,
                UserName = o.customer.UserName,
                address = o.address,
                phone = o.phone,
                TotalPrice = o.TotalPrice
            }).ToList();
            return Ok(orderDTOs);

        }



        
        [HttpGet("GetOrderDetails")]
        [Authorize(Roles = "DeliveryRep")]
        public async Task<IActionResult> GetOrderDetails(  int orderId)
                   {

          var order = await _context.Orders
                .Where(o => (o.Id == orderId ))
                .Include(o => o.customer)
                .Include(o => o.orderItems)
                    .ThenInclude(oi => oi.product)
                 .Include(o => o.orderItems)
                    .ThenInclude(oi => oi.color)
                 .Include(o => o.orderItems)
                    .ThenInclude(oi => oi.size)
                 .Include(o => o.orderItems)
                    .ThenInclude(o => o.product)
                        .ThenInclude(o => o.images)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = "Order not found." });

            var orderDTO = new specificOrderDto
            {
                Id = order.Id,
                status = order.Status.ToString(),
                CustomerId = order.CustomerId,
                UserName = order.customer.UserName,
                address = order.address,
                phone = order.phone,
                // All unique product IDs in this order
                ProductId = order.orderItems.Select(oi => oi.product.Id).ToArray(),
                ProductsName = order.orderItems.Select(oi => oi.product.Title).ToArray(),
                unitprice = order.orderItems.Select(oi => oi.UnitPrice).ToArray(),
                TotalPrice = order.TotalPrice,
                // All unique quantities in this order
                quantity = order.orderItems.Select(oi => oi.Quantity).ToArray(),

                // All unique colors in this order
                color = order.orderItems.Select(oi => oi.color.Name).ToArray(),
                // All unique sizes in this order
                size = order.orderItems.Select(oi => oi.size.Gradient).ToArray(),
                // All unique images in this order
                image = order.orderItems.Select(oi => oi.product.images.FirstOrDefault().ImageData).ToArray()
            };

            for (int i = 0; i < orderDTO.image.Length; i++)
            {
                orderDTO.image[i] = $"//aston.runasp.net//Product_Image//{orderDTO.image[i] ?? "unknownProduct.jpg"}";
            }

            return Ok(orderDTO);
        }



        //  Update Order Status
        [HttpPut("UpdateOrderStatus")]
        [Authorize(Roles = "DeliveryRep")]
        public async Task<IActionResult> UpdateOrderStatus( [FromBody] UpdateOrderStatusDTO dto)
        {


            if (!Enum.TryParse<OrdStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            var order = await _context.Orders.Include(o => o.orderItems)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId );

            if (order == null)
                return NotFound(new { message = "Order not found." });

            if (order.Status == OrdStatus.Pending)
                return BadRequest(new { message = "Order is pending. Delivery Rep Cannot update status." });

            if (order.Status == OrdStatus.Recieved)
                return BadRequest(new { message = "Order is Already Recieved and can't update Status Conntact to Admin if you need" });

            //  Check allowed status transitions
            bool isValidTransition =
                (order.Status == OrdStatus.Preparing && newStatus == OrdStatus.OnWay) ||
                (order.Status == OrdStatus.OnWay && newStatus == OrdStatus.Recieved);



            if (!isValidTransition)
                return BadRequest(new { message = "Invalid status transition. Allowed: Preparing → OnWay → Recieved." });




            foreach (var item in order.orderItems)
            {
                item.Status = newStatus;
            }





            //  Notify customer by email
            if (order.Status == OrdStatus.OnWay)
            {
                
                var customer = await _context.Users.FindAsync(order.CustomerId);
                if (customer != null)
                {
                    await _emailService.SendEmailAsync(
                        customer.Email,
                        $"Your Order Status {order.Status}",
                        $"Dear {customer.UserName}, your order : {newStatus}."
                    );
                }

            }
            if (order.Status == OrdStatus.Recieved)
            {
               
                var customer = await _context.Users.FindAsync(order.CustomerId);
                if (customer != null)
                {
                    order.DeliveryDate = DateTime.Now;
                    await _emailService.SendEmailAsync(
                        customer.Email,
                        $"Your Order Status {order.Status}",
                        $"Dear {customer.UserName}, your order : {newStatus} At {order.DeliveryDate}."
                    );
                }

            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Order status updated to {newStatus}.",
                OrderId = order.Id,
                NewStatus = newStatus.ToString()
            });
        }
    }
}
