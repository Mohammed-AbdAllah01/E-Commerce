using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using Project.DTOs;
using Project.Enums;
using Project.Services.Interfaces;
using Project.Tables;

namespace Project.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : ControllerBase {
        private readonly AppDbContext context;
        private readonly IEmailService emailService;
        public MerchantController(AppDbContext context, IEmailService emailService) {
            this.context = context;
            this.emailService = emailService;

        }


        [HttpGet("ShowAllMerchants")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowAllMerchants() {
            var merchants = context.Merchants.Include(m => m.orderItems).ToList();
            var result = new List<ShowMerchantDTO>();
            foreach (var merchant in merchants) {
                var merchantDTO = new ShowMerchantDTO {
                    MerchantId = merchant.Id,
                    MerchantName = merchant.UserName,
                    Feedback = merchant.Feedback,
                    Status = merchant.Status.ToString(),
                    HireAge = merchant.HireAge
                };
                var totalGain = 0.0;
                foreach (var orderItem in merchant.orderItems) {
                    if (orderItem.Status == OrdStatus.Recieved) {
                        totalGain += orderItem.product.SellPrice * orderItem.product.Quantity;
                    }
                }
                merchantDTO.GainMoney = totalGain;
                result.Add(merchantDTO);
            }
            return Ok(result);
        }

        [HttpGet("ShowMerchantwithUserName")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowMerchantwithUserName(string UserName)
        {
            var merchants = context.Merchants.Where(e => e.UserName == UserName).Include(m => m.orderItems).ToList();
            if (merchants == null || merchants.Count == 0)
            {
                return NotFound("No merchants found with the given username.");
            }
            var result = new List<ShowMerchantDTO>();
            foreach (var merchant in merchants)
            {
                var merchantDTO = new ShowMerchantDTO
                {
                    MerchantId = merchant.Id,
                    MerchantName = merchant.UserName,
                    Feedback = merchant.Feedback,
                    Status = merchant.Status.ToString(),
                    HireAge = merchant.HireAge
                };
                var totalGain = 0.0;
                foreach (var orderItem in merchant.orderItems)
                {
                    if (orderItem.Status == OrdStatus.Recieved)
                    {
                        totalGain += orderItem.product.SellPrice * orderItem.product.Quantity;
                    }
                }
                merchantDTO.GainMoney = totalGain;
                result.Add(merchantDTO);
            }
            return Ok(result);
        }


        [HttpPut("UpdateStatus")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateMerchantStatus(string merchantId, string status) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            // Check if the product exists
            var merchant = context.Merchants.FirstOrDefault(f => f.Id == merchantId);
            if (merchant == null) {
                return NotFound("Merchant not found");
            }
            if (Enum.TryParse(status, out AccStatus parsedStatus)) {
                merchant.Status = parsedStatus;
                context.SaveChanges();
                return Ok("Merchant status updated");
            }
            else {
                return BadRequest("Invalid status value");
            }
        }

        //---------------------------------Merchant--------------------------------------------------------

        [HttpGet("GetOrderItems")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetOrderItems(string merchantId)
        {
            var orderItems = await context.OrderItems
                .Include(o => o.order)
                .Include(o => o.product)
                .Include(pd => pd.color)
                .Include(pd => pd.size)
                .Include(pd => pd.merchant)
                .Where(o => o.MerchantId == merchantId)
                .ToListAsync();
            if (orderItems == null || orderItems.Count == 0)
            {
                return NotFound("No order items found for this merchant.");
            }
            var result = orderItems.Select(o => new OrderItemDTO
            {
                Id = o.Id,
                ProductName = orderItems.Select(oi => oi.product.Title).FirstOrDefault(),
                Color = o.color.Name,
                MerchantName = o.merchant.UserName,
                Size = o.size.Gradient,
                Quantity = o.Quantity,
                Status = o.Status.ToString(),
                Price = o.product.SellPrice
            }).ToList();
            return Ok(result);
        }

        //  Update Order Status
        [HttpPut("UpdateOrderItemStatus")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateOrderItemStatus([FromBody] UpdateOrderItemStatusDTO dto)
        {


            if (!Enum.TryParse<OrdStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest("Invalid status value.");

            var orderitem = await context.OrderItems.Include(o => o.order)
                .FirstOrDefaultAsync(o => o.Id == dto.Id);

            if (orderitem == null)
                return NotFound("orderitem not found.");

            var oldStatus = orderitem.order.Status;

            if (orderitem.Status != OrdStatus.Pending)
                return BadRequest("Order item is not pending. Merchant Cannot update status.");
            if (newStatus != OrdStatus.Preparing)
                return BadRequest("Invalid status transition. Allowed: Pending → Preparing only.");

            else if (orderitem.Status == OrdStatus.Pending)
                orderitem.Status = newStatus;



            var Deliver = await context.Users.FindAsync(orderitem.order.DeliveryId);
            if (Deliver != null && oldStatus != newStatus)
            {
                await emailService.SendEmailAsync(
                    Deliver.Email,
                    $"Order {orderitem.orderId} Status {orderitem.order.Status}",
                    $"Dear {Deliver.UserName}, order be : {newStatus}."
                ); }


            await context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Order status updated to {newStatus}.",
                OrderId = orderitem.order.Id,
                NewStatus = dto.NewStatus
            });

        }


        [HttpGet("GetProduct")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetProduct(string MerchantId)
        {
            var products = await context.Products
                .Include(p => p.images)
                .Include(p => p.category)
                .Include(p => p.ProductDetails)
                .Include(p => p.merchant)
                .Where(p => p.merchantId == MerchantId)
                .ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for this merchant.");
            }
            var result = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                title = p.Title,
                Description = p.Description,
                CategoryName = p.category.Name,
                Discount = p.Discount,
                Unite = p.UnitPrice,
                SellPrice = p.SellPrice,
                status = p.Status,
                Quantity = p.Quantity,
                Image = p.images.FirstOrDefault()?.ImageData
            }).ToList();
            return Ok(result);

        }


        [HttpGet("GetProductDetails")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetProductDetails(int ProductId)
        {
            var productDetails = await context.ProductDetails
                .Include(p => p.product)
                    .ThenInclude(p => p.images)
                .Include(p => p.product)
                .Include(p => p.color)
                .Include(p => p.size)
                .Where(p => p.productId == ProductId)
                .ToListAsync();
            if (productDetails == null || productDetails.Count == 0)
            {
                return NotFound("No product details found for this merchant.");
            }
            var result = productDetails.Select(p => new ProductDetailDTO
            {
                Id = p.Id,
                ProductName = p.product.Title,
                Color = p.color.Name,
                Size = p.size.Gradient,
                Quantity = p.Quantity,
                Image = p.product.images.FirstOrDefault()?.ImageData

            }).ToList();
            return Ok(result);
        }



        [HttpPut("UpdateProductQuantity")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateProductQuantity(int ProductDetailId, int newQuantity)
        {
            if (newQuantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }
            var productDetail = await context.ProductDetails.Include(p => p.product)
                .FirstOrDefaultAsync(o => o.Id == ProductDetailId);
            if (productDetail == null)
            {
                return NotFound("Product item not found.");
            }

            productDetail.Quantity = newQuantity;
            if (productDetail.Quantity > 0) {
                productDetail.product.Status = ProStatus.Active;
            }
            else
            {
                productDetail.product.Status = ProStatus.OutOfStock;
            }
            await context.SaveChangesAsync();
            return Ok($"Product quantity updated successfully to be {newQuantity}.");
        }


        [HttpPut("UpdateProduct")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateProduct(EditProductDTO EP)
        {
            if (EP == null)
            {
                return BadRequest("Product cannot be null.");
            }
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == EP.Id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            if (EP.Title == null && EP.Description == null && EP.Discount == null && EP.UnitPrice == null)
            {
                return BadRequest("No fields to update.");
            }

            if (EP.Title != null)
            {
                product.Title = EP.Title;
            }
            if (EP.Description != null)
            {
                product.Description = EP.Description;
            }
            if (EP.Discount != null)
            {
                product.Discount = EP.Discount;
            }
            if (EP.UnitPrice != null)
            {
                product.UnitPrice = EP.UnitPrice;
            }
            product.CalculateSellPrice();
            await context.SaveChangesAsync();

            return Ok("Product updated successfully.");

        }

        




    }
}
