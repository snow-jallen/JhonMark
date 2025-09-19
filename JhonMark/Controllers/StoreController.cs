using JhonMark.ClientModels;
using JhonMark.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JhonMark.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly StoreContext context;

        public StoreController(StoreContext context)
        {
            this.context = context;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct(AddProductRequest addProductRequest)
        {
            var product = new Product
            {
                Name = addProductRequest.Name,
                Price = addProductRequest.Price,
                Description = addProductRequest.Description
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id });
        }

    }
}
