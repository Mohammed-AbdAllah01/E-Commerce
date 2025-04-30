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
            var merchants = context.Merchants
                .Include(m => m.products)
                    .ThenInclude(p => p.feedbacks)
                .Include(m => m.orderItems)
                   
                .ToList();
            var result = merchants.Select(p => new ShowMerchantDTO
            {
                MerchantId = p.Id,
                MerchantName = p.UserName,
                Feedback = p.Feedback,
                Status = p.Status.ToString(),
                HireAge = p.HireAge,
               GainMoney = p.orderItems.Where(oi => oi.Status == OrdStatus.Recieved)
                    .Sum(oi => oi.product.SellPrice * oi.Quantity)
            }).ToList();
            return Ok(result);
        }

        [HttpGet("ShowMerchantwithId")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowMerchantwithUserName(string Id)
        {
            var merchants = context.Merchants
                .Where(e => e.Id == Id).
                Include(m => m.products)
                 .ThenInclude(p => p.feedbacks)
                .Include(m => m.orderItems).ToList();
            if (merchants == null || merchants.Count == 0)
            {
                return NotFound("No merchants found with the given username.");
            }
            var result = merchants.Select(p => new ShowMerchantDTO
            {
                MerchantId = p.Id,
                MerchantName = p.UserName,
                Feedback = p.Feedback,
                Status = p.Status.ToString(),
                HireAge = p.HireAge,
                GainMoney = p.orderItems.Where(oi => oi.Status == OrdStatus.Recieved)
                    .Sum(oi => oi.product.SellPrice * oi.Quantity)
            });
            return Ok(result);
           
        }


        [HttpPut("UpdateMerchantStatus")]
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
                if (parsedStatus != merchant.Status)
                {
                    merchant.Status = parsedStatus;
                context.SaveChanges();
                return Ok("Merchant status updated");
            }
                else
                {
                    return BadRequest("Merchant status is already the same");
                }
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
                Image = $"//aston.runasp.net//Profile_Image//{p.images.FirstOrDefault()?.ImageData}"
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
                Image = $"//aston.runasp.net//Profile_Image//{p.product.images.FirstOrDefault()?.ImageData}"
                
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


        //---------------------------------------------
        [HttpPost("AddProduct")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> AddProduct([FromBody] AddFullProductDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var merchantId = User.FindFirst("ID")?.Value;
            if (merchantId == null)
                return Unauthorized("Unauthorized");

            var category = await context.Categories.FindAsync(model.CategoryId);
            if (category == null)
                return NotFound("Invalid category");

            var product = new Product
            {
                Title = model.Title,
                Description = model.Description,
                Discount = model.Discount,
                UnitPrice = model.UnitPrice,
                categoryId = model.CategoryId,
                merchantId = merchantId,
                Status = ProStatus.Active,
                //  Feedback = 0

            };

            context.Products.Add(product);
            await context.SaveChangesAsync();


            foreach (var colorImg in model.ColorImages)
            {
                foreach (var img in colorImg.ImageUrls)
                {
                    context.Images.Add(new Image
                    {
                        productId = product.Id,
                        colorId = colorImg.ColorId,
                        ImageData = img,
                        product = product,
                        color = await context.Colors.FindAsync(colorImg.ColorId) ?? throw new Exception("Invalid color")
                    });
                }
            }

            //  Add (color, size, quantity)
            foreach (var detail in model.ProductDetails)
            {
                var color = await context.Colors.FindAsync(detail.ColorId);
                var size = await context.Sizes.FindAsync(detail.SizeId);
                if (color == null || size == null)
                    return BadRequest("Invalid color or size.");

                context.ProductDetails.Add(new ProductDetail
                {
                    productId = product.Id,
                    colorId = detail.ColorId,
                    sizeId = detail.SizeId,
                    Quantity = detail.Quantity,
                    product = product,
                    color = color,
                    size = size
                });
            }

            await context.SaveChangesAsync();

            // send mail to customers who add this merchant to favourite
            var favCustomers = await context.FavMerchants
                .Include(fm => fm.customer)
                .Where(fm => fm.merchantId == merchantId)
                .Select(fm => fm.customer)
                .ToListAsync();

            foreach (var customer in favCustomers)
            {
                await emailService.SendEmailAsync(
                    customer.Email,
                    "New Product Added",
                    $"Dear {customer.UserName},\n\nThe merchant you follow just added a new product: {product.Title}.\nCheck it out now!"
                );
            }

            return Ok(new
            {
                message = "✅ Product added successfully and notifications sent.",
                productId = product.Id
            });
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

