using DataBase.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using Project.DTOs;
using Project.Enums;
using Project.Tables;

namespace Project.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController: ControllerBase {
        private readonly AppDbContext context;

        public OrderController(AppDbContext context, UserManager<Person> userManager) {
            this.context = context;
            UserManager = userManager;
        }

        public UserManager<Person> UserManager { get; }

        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder(AddOrderDTO addOrderDTO)
        {
            if (addOrderDTO == null)
            {
                return BadRequest(new { message = "Invalid order data" });
            }
            if (string.IsNullOrEmpty(addOrderDTO.CustomerId) ||
                string.IsNullOrEmpty(addOrderDTO.Address) ||
                string.IsNullOrEmpty(addOrderDTO.Phone))
            {
                return BadRequest(new { message = "CustomerId, Address and Phone are required" });
            }
            var carts = context.Carts.Where(c => c.customerId == addOrderDTO.CustomerId)
                                    .ToList();
            var customer = await UserManager.FindByIdAsync(addOrderDTO.CustomerId);
            if (carts.Count == 0)
            {
                return BadRequest(new { message = "Cart not found" });
            }
            if (customer == null)
            {
                return BadRequest(new { message = "Customer not found" });
            }

            var randomDelivery = context.DeliveryReps
                                   .OrderBy(s => Guid.NewGuid())
                                   .FirstOrDefault();
            var totalPrice = 0.0 ;
            foreach (var cart in carts)
            {
                var product = context.Products.Include(p => p.ProductDetails).FirstOrDefault(p => p.Id == cart.productId);
                if (product == null)
                {
                    return BadRequest(new { message = "Product not found" });
                }
                totalPrice += cart.Quantity * product.SellPrice;
            }

                var order = new Order()
            {
                address = addOrderDTO.Address,
                CustomerId = customer.Id,
                DeliveryId = randomDelivery.Id,
                OrderDate = DateTime.Now,
                phone = addOrderDTO.Phone,
                TotalPrice = totalPrice,
                };

            context.Orders.Add(order);
            context.SaveChanges();

            foreach (var cart in carts)
            {
                var product = context.Products.Include(p => p.ProductDetails).FirstOrDefault(p => p.Id == cart.productId);
                if (product == null)
                {
                    return BadRequest(new { message = "Product not found" });
                }
                var productDetails = context.ProductDetails
                    .Include(p => p.color)
                    .Include(p => p.size)
                    .FirstOrDefault(p => p.productId == cart.productId && p.colorId == cart.colorId && p.sizeId == cart.sizeId);
                if (productDetails == null)
                {
                    return BadRequest(new { message = "Product details not found" });
                }
                if (product.Quantity == 0 || product.Status == ProStatus.OutOfStock)
                {
                    return BadRequest(new { message = "Product is out of stock" });
                }
                if (cart.Quantity > productDetails.Quantity)
                {
                    return BadRequest(new { message = "Not enough quantity" });
                }
                if (product.Status == ProStatus.Pending || product.Status == ProStatus.Banned)
                {
                    return BadRequest(new { message = "Product is not active now" });
                }
                

                var orderItems = new OrderItem()
                {
                    color = productDetails.color,
                    MerchantId = product.merchantId,
                    order = order,
                    product = product,
                    size = productDetails.size,
                    Quantity = cart.Quantity,
                    colorId = productDetails.colorId,
                    sizeId = productDetails.sizeId,
                    productId = productDetails.productId,
                    Status = OrdStatus.Pending,
                    orderId = order.Id,
                    UnitPrice = product.SellPrice
                };

                productDetails.Quantity -= cart.Quantity;
                context.ProductDetails.Update(productDetails);


               
                if (product.Quantity == 0)
                {
                    product.Status = ProStatus.OutOfStock;
                    context.Products.Update(product);
                }

                context.OrderItems.Add(orderItems);
                context.Carts.Remove(cart);
                context.SaveChanges();
            }
            return Ok(new { message = "Cart added to order successfully" });
        }
    }
}
