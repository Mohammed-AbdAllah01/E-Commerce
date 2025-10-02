using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project.Data.Relation;
using Project.DTOs;
using Project.Enums;
using Project.Services.Implementations;
using Project.Services.Interfaces;
using Project.Tables;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using Image = Project.Data.Relation.Image;


namespace Project.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : ControllerBase {
        private readonly AppDbContext context;
        private readonly IEmailService emailService;
        private readonly string _profileImagePath;
        public MerchantController(AppDbContext context, IEmailService emailService) {
            this.context = context;
            this.emailService = emailService;
            _profileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Product_Image");

            if (!Directory.Exists(_profileImagePath))
            {
                Directory.CreateDirectory(_profileImagePath);
            }

        }


        [HttpGet("ShowAllMerchants")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowAllMerchants() {
            var merchants = context.Merchants
                .Include(m => m.products)
                    .ThenInclude(p => p.feedbacks)
                .Include(m => m.orderItems)
                .ToList();
            if (merchants == null || merchants.Count == 0)
            {
                return NotFound(new { message = "No merchants found." });
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
                return NotFound(new { message = "No merchants found with the given username." });
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
                return NotFound(new { message = "Merchant not found" });
            }
            if (Enum.TryParse(status, out AccStatus parsedStatus)) {
                if (parsedStatus != merchant.Status)
                {
                    merchant.Status = parsedStatus;
                    context.SaveChanges();
                    return Ok(new { message = "Merchant status updated" });
                }
                else
                {
                    return BadRequest(new { message = "Merchant status is already the same" });
                }
            }
            else {
                return BadRequest(new { message = "Invalid status value" });
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
                    .ThenInclude(p => p.images)
                .Include(pd => pd.color)
                .Include(pd => pd.size)
                .Include(pd => pd.merchant)
                .Where(o => o.MerchantId == merchantId)
                .ToListAsync();
            if (orderItems == null || orderItems.Count == 0)
            {
                return NotFound(new { message = "No order items found for this merchant." });
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
                Price = o.product.SellPrice,
                ImageUrl = $"//aston.runasp.net//Product_Image//{o.product.images.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();
            return Ok(result);
        }

        //  Update Order Status
        [HttpPut("UpdateOrderItemStatus")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateOrderItemStatus([FromBody] UpdateOrderItemStatusDTO dto)
        {


            if (!Enum.TryParse<OrdStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            var orderitem = await context.OrderItems.Include(o => o.order)
                .FirstOrDefaultAsync(o => o.Id == dto.Id);

            if (orderitem == null)
                return NotFound(new { message = "orderitem not found." });

            var oldStatus = orderitem.order.Status;

            if (orderitem.Status != OrdStatus.Pending)
                return BadRequest(new { message = "Order item is not pending. Merchant Cannot update status." });
            if (newStatus != OrdStatus.Preparing)
                return BadRequest(new { message = "Invalid status transition. Allowed: Pending → Preparing only." });

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
                return NotFound(new { message = "No products found for this merchant." });
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
                status = p.Status.ToString(),
                Quantity = p.Quantity,
                Image = $"//aston.runasp.net//Product_Image//{p.images.FirstOrDefault(e => e.productId == p.Id && e.colorId == p.ProductDetails.FirstOrDefault().colorId)?.ImageData ?? "unknownProduct.jpg"}"
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
                return NotFound(new { message = "No product details found for this Product." });
            }
            var result = productDetails.Select(p => new ProductDetailDTO
            {
                Id = p.Id,
                ProductName = p.product.Title,
                Color = p.color.Name,
                Size = p.size.Gradient,
                Quantity = p.Quantity,
                Image = $"//aston.runasp.net//Product_Image//{p.product.images.FirstOrDefault(e => e.productId == p.productId && e.colorId == p.colorId)?.ImageData ?? "unknownProduct.jpg"}"

            }).ToList();
            return Ok(result);
        }



        [HttpPut("UpdateProductQuantity")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateProductQuantity(int ProductDetailId, int newQuantity)
        {
            if (newQuantity < 0)
            {
                return BadRequest(new { message = "Quantity cannot be negative." });
            }
            var productDetail = await context.ProductDetails.Include(p => p.product)
                .FirstOrDefaultAsync(o => o.Id == ProductDetailId);
            if (productDetail == null)
            {
                return NotFound(new { message = "Product item not found." });
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
            return Ok(new { message = $"Product quantity updated successfully to be {newQuantity}."});
        }


        [HttpPut("UpdateProduct")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UpdateProduct(EditProductDTO EP)
        {
            if (EP == null)
            {
                return BadRequest(new { message = "Product cannot be null." });
            }
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == EP.Id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }
            if (EP.Title == null && EP.Description == null && EP.Discount == null && EP.UnitPrice == null)
            {
                return BadRequest(new { message = "No fields to update." });
            }

            if (EP.Title != null && EP.Title != product.Title)
            {
                product.Title = EP.Title;
            }
            if (EP.Description != null && EP.Description != product.Description)
            {
                product.Description = EP.Description;
            }
            if (EP.Discount != null && EP.Discount != product.Discount)
            {
                product.Discount = EP.Discount;
            }
            if (EP.UnitPrice != null && EP.UnitPrice != product.UnitPrice)
            {
                product.UnitPrice = EP.UnitPrice;
            }
            product.CalculateSellPrice();
            await context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully." });

        }


        //---------------------------------------------

        [HttpPost("AddProduct")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> AddProduct(AddFullProductDTO Pro )
        {
            if (Pro == null)
            {
                return BadRequest(new { message = "Product cannot be null." });
            }
            if (string.IsNullOrEmpty(Pro.Title) || string.IsNullOrEmpty(Pro.Description) ||
                string.IsNullOrEmpty(Pro.merchantId) || Pro.Discount < 0 ||
                string.IsNullOrEmpty(Pro.CategoryName) || Pro.UnitPrice <= 0)
            {
                return BadRequest(new { message = "Invalid product data." });
            }
            var merchant = await context.Merchants.FindAsync(Pro.merchantId);
            if (merchant == null)
            {
                return NotFound(new { message = "Merchant not found." });
            }
            var scat = Pro.CategoryName.ToLower();
            var category = await context.Categories.FirstOrDefaultAsync( e=> e.Name == scat);
            if (category == null)
            {
                category = new Category{ Name = scat , Type = Pro.Type };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            var exist = await context.Products
                .FirstOrDefaultAsync(p => p.Title == Pro.Title && p.Description == Pro.Description);
            if (exist != null)
            {
                return BadRequest(new { message = "Product with this title and Description already exists." });
            }
            var product = new Product
            {
                Title = Pro.Title,
                Description = Pro.Description,
                UnitPrice = Pro.UnitPrice,
                Discount = Pro.Discount,
                categoryId = category.Id,
                merchantId = Pro.merchantId,
                Status = ProStatus.Pending
            };
            product.CalculateSellPrice();
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return Ok(new { productId = product.Id });
        }

        [HttpPost("AddColorsizeimage")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> AddColorsizeimage([FromForm] ColorSizeDTO details)
        {

            if (details == null)
            {
                return BadRequest(new { message = "Product cannot be null." });
            }
            if (string.IsNullOrEmpty(details.Color))
            {
                return BadRequest(new { message = "Invalid please Enter Color." });

            }
            var product = await context.Products.FindAsync(details.ProductId);
            if (details.ProductId == null || product == null)
            {
                return BadRequest(new { message = "Invalid product ID ." });

            }
            foreach (var Quantity in details.Quantity)
            {
                if (Quantity < 0)
                {
                    return BadRequest(new { message = "Invalid please Enter Quantity." });
                }
            }
            foreach (var size in details.Size)
            {
                if (string.IsNullOrEmpty(size))
                {
                    return BadRequest(new { message = "Invalid please Enter size." });
                }
            }
            string imageUrl1 = null;
            string imageUrl2 = null;
            string imageUrl3 = null;
            if (details.image1 != null && details.image1.Length > 0)
            {
                imageUrl1 = SaveImage(details.image1);
            }
            if (details.image2 != null && details.image2.Length > 0)
            {
                imageUrl2 = SaveImage(details.image2);
            }
            if (details.image3 != null && details.image3.Length > 0)
            {
                imageUrl3 = SaveImage(details.image3);
            }
            var scolor = details.Color.ToLower();

            var color = await context.Colors.FirstOrDefaultAsync(c => c.Name == scolor);
            if (color == null)
            {
                color = new Tables.Color { Name = scolor };
                context.Colors.Add(color);
                await context.SaveChangesAsync();
            }
            var sizeList = new List<Tables.Size>();
            foreach (var size in details.Size)
            {
                var ssize = size.ToLower();
                var existingSize = await context.Sizes.FirstOrDefaultAsync(s => s.Gradient == ssize);
                if (existingSize == null)
                {
                    existingSize = new Tables.Size { Gradient = ssize };
                    context.Sizes.Add(existingSize);
                    await context.SaveChangesAsync();
                }
                sizeList.Add(existingSize);
            }
            var count = 0;
            for (int i = 0; i < sizeList.Count; i++)
            {
                var size = sizeList[i];
                var quantity = details.Quantity[i];
                var existdetails = await context.ProductDetails
                    .FirstOrDefaultAsync(pd => pd.productId == details.ProductId && pd.colorId == color.Id && pd.sizeId == size.Id);
                if (existdetails == null)
                {
                    var productDetail = new ProductDetail
                    {
                        productId = details.ProductId,
                        colorId = color.Id,
                        sizeId = size.Id,
                        Quantity = quantity
                    };
                    count++;
                    context.ProductDetails.Add(productDetail);
                    await context.SaveChangesAsync();
                }
            }
            if (count == 0)
            {
                return BadRequest(new { message = "Product details already exist for this color and size." });
            }
                        
            if (!string.IsNullOrEmpty(imageUrl1))
            {
                var productImage1 = new Image
                {
                    ImageData = imageUrl1,
                    productId = details.ProductId,
                    colorId = color.Id
                };
                context.Images.Add(productImage1);
            }

            if (!string.IsNullOrEmpty(imageUrl2))
            {
                var productImage2 = new Image
                {
                    ImageData = imageUrl2,
                    productId = details.ProductId,
                    colorId = color.Id
                };
                context.Images.Add(productImage2);
            }

            if (!string.IsNullOrEmpty(imageUrl3))
            {
                var productImage3 = new Image
                {
                    ImageData = imageUrl3,
                    productId = details.ProductId,
                    colorId = color.Id
                };
                context.Images.Add(productImage3);
            }
            await context.SaveChangesAsync();
            return Ok(new { message = "Product added successfully." });
        }

        







        private string SaveImage(IFormFile imageFile)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(_profileImagePath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            return uniqueFileName; // URL path  {"//aston.runasp.net//Product_Image//" +}
        }


    }



}

