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
                return NotFound("No customers found with the given username.");
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
                return NotFound("Customer not found");
            }
            if (Enum.TryParse(status, out AccStatus customerStatus))
            {
                if (customerStatus != customer.Status)
                {
                    customer.Status = customerStatus;
                    context.SaveChanges();
                    return Ok("Customer status updated successfully");
                }
                else
                {
                    return BadRequest("Customer status is already the same");
                }
            }
            return BadRequest("Invalid status");
            }



        [HttpPost("AddComment")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddComment([FromBody] AddProductCommentDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerId = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized("Customer not found in token");

            // ensure tha the customer received the order
            var hasReceived = await context.OrderItems
                .Include(oi => oi.order)
                .Where(oi =>
                    oi.productId == model.ProductId &&
                    oi.order.CustomerId == customerId &&
                    oi.Status == OrdStatus.Recieved
                ).AnyAsync();

            if (!hasReceived)
                return BadRequest("❌ You can only comment on products you've received.");

            // ensure if there is a previous comment
            var existingComment = await context.FeedbackComments
                .AnyAsync(c => c.productId == model.ProductId && c.customerId == customerId);

            if (existingComment)
                return BadRequest("❌ You've already commented on this product.");


            var product = await context.Products
                .Include(p => p.merchant)
                .Include(p => p.feedbackcmments)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product == null)
                return NotFound("❌ Product not found.");

            var customer = await context.Customers.FindAsync(customerId);
            if (customer == null)
                return NotFound("Customer not found.");

            // create comment
            var comment = new FeedbackComments
            {
                productId = product.Id,
                customerId = customer.Id,
                Comment = model.Comment,
                Feeling = model.Feedback,
                DateCreate = DateTime.UtcNow,
                product = product,
                customer = customer
            };

            context.FeedbackComments.Add(comment);
            await context.SaveChangesAsync();


            var allRatings = product.feedbackcmments.Select(fc => fc.Feeling).ToList();
            var averageRating = allRatings.Any() ? allRatings.Average() : model.Feedback;


            typeof(Product).GetProperty("Feedback")?.SetValue(product, averageRating);
            await context.SaveChangesAsync();

            // send mail to the merchant
            await emailService.SendEmailAsync(
                product.merchant.Email,
                "🛍 New Feedback on Your Product",
                $"Dear {product.merchant.UserName},\n\nA customer has left feedback on your product \"{product.Title}\".\n\n📝 Comment: \"{model.Comment}\"\n⭐ Rating: {model.Feedback} stars\n\nRegards,\nYour Platform"
            );

            return Ok("✅ Feedback submitted successfully.");
        }

    }
}

