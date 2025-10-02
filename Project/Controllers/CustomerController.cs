using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using Project.DTOs;
using Project.Enums;
using Project.Services.Implementations;
using Project.Services.Interfaces;
using Project.Tables;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IEmailService emailService;

        public CustomerController(AppDbContext context, IEmailService emailService)
        {
            this.context = context;
            this.emailService = emailService;
        }

        [HttpGet("ShowAllCustomers")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowAllCustomers()
        {
            var customers = context.Customers.
                Include(c => c.Orders)
                .ToList();
            if (customers == null || customers.Count == 0)
            {
                return NotFound(new { message = "No customers found." });
            }

            var result = customers.Select(p => new ShowCustomerDTO
            {
                CustomerId = p.Id,
                CustomerName = p.UserName,
                Status = p.Status.ToString(),
                ordercount = p.Orders.Count
            }).ToList();
            return Ok(result);
        }

        [HttpGet("ShowCustomerwithId")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowCustomerwithUserName(string Id)
        {
            var customers = context.Customers.
                 Include(c => c.Orders)
                .Where(e => e.Id == Id).ToList();
            if (customers == null || customers.Count == 0)
            {
                return NotFound(new { message = "No customers found with the given username." });
            }
            var result = customers.Select(p => new ShowCustomerDTO
            {
                CustomerId = p.Id,
                CustomerName = p.UserName,
                Status = p.Status.ToString(),
                ordercount = p.Orders.Count
            });

            return Ok(result);
        }

        [HttpPut("UpdateCustomerStatus")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateCustomerStatus(string customerId, string status)
        {
            var customer = context.Customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }
            if (Enum.TryParse(status, out AccStatus customerStatus))
            {
                if (customerStatus != customer.Status)
                {
                    customer.Status = customerStatus;
                    context.SaveChanges();
                    return Ok(new { message = "Customer status updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Customer status is already the same" });
                }
            }
            return BadRequest(new { message = "Invalid status" });
        }



        [HttpPost("SaveCustomerLogs")]
        public async Task<IActionResult> SaveCustomerLogs(LogsDTO log)
        {
           
                if (!Enum.TryParse<EventStatus>(log.event_type, true, out var newStatus))
                    return BadRequest(new { message = "Invalid status value." });

                var customer = await context.Customers.FindAsync(log.user_id);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }
                var product = await context.Products.FindAsync(log.product_id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
            // Check if a log already exists for this customer and product
            var existingLog = await context.Histories
                .FirstOrDefaultAsync(h => h.customerId == log.user_id && h.productId == log.product_id && h.event_type == newStatus);
            if (existingLog != null)
            {
                return Ok(new { message = "Log already exists for this customer and product with the same status." });
            }
            var newLog = new History
                {
                    customerId = log.user_id,
                    productId = log.product_id,
                    event_type = newStatus
                };
                context.Histories.Add(newLog);
                await context.SaveChangesAsync();
            
            return Ok(new { message = "Log saved successfully" });

        }



        [HttpGet("ShowCustomerLogs")]
        public IActionResult ShowCustomerLogs()
        {                      
            
            var logs = context.Histories.Select(h => new LogsDTO
            {
                user_id = h.customerId,
                product_id = h.productId,
                event_type = h.event_type.ToString()
            }).ToList();
            return Ok(logs);

        }




        [HttpGet("ShowCustomerOrders")]
        public async Task<IActionResult> ShowCustomerOrders(string customerId)
        {
            try
            {
                var customer = context.Customers.FirstOrDefault(c => c.Id == customerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }


                var orders = await context.Orders
                    .Where(o => (o.CustomerId == customerId))
                    .Include(o => o.customer)
                    .Include(o => o.orderItems).
                    Include(o => o.deliveryrep)
                    .ToListAsync();
                if (orders == null || orders.Count == 0)
                    return BadRequest(new { message = "No orders found for this delivery person." });
                var orderDTOs = orders.Select(o => new OrdercusDTO
                {
                    OrderId = o.Id,
                    status = o.Status.ToString(),
                    UserName = o.customer.UserName,
                    address = o.address,
                    phone = o.phone,
                    DeliveryName = o.deliveryrep.UserName
                }).ToList();
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while retrieving orders: " + ex.Message });
            }
        }




        [HttpGet("GetCustomerOrderDetails")]        
        public async Task<IActionResult> GetCustomerOrderDetails(int orderId)
        {
            var Order = await context.Orders.FindAsync(orderId);
            if (Order == null)
                return NotFound(new { message = "Order not found." });

            var order = await context.Orders
                  .Where(o => (o.Id == orderId))
                  .Include(o => o.customer)
                  .Include(o => o.deliveryrep)
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

            var orderDTO = new specificOrdercusDto
            {
                OrderId = order.Id,
                status = order.orderItems.Select(oi => oi.Status.ToString()).ToArray(),
                UserName = order.customer.UserName,
                address = order.address,
                phone = order.phone,
                // All unique product IDs in this order
                ProductId = order.orderItems.Select(oi => oi.product.Id).ToArray(),
                ProductsName = order.orderItems.Select(oi => oi.product.Title).ToArray(),
                // All unique colors in this order
                color = order.orderItems.Select(oi => oi.color.Name).ToArray(),
                // All unique sizes in this order
                size = order.orderItems.Select(oi => oi.size.Gradient).ToArray(),
                unitprice = order.orderItems.Select(oi => oi.UnitPrice).ToArray(),
                quantity = order.orderItems.Select(oi => oi.Quantity).ToArray(),
                TotalPrice = order.TotalPrice,
                // All unique quantities in this order
               
                DeliveryName = order.deliveryrep.UserName

            };

            return Ok(orderDTO);
        }


    }
}

