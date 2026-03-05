using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data.Interfaces;
using WebApplication2.Models;
using WebApplication2.DTOs;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // GET запросы
        // ========================================

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync();

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка продуктов");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new { message = "Продукт не найден" });
                }

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении продукта с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/products/category/1
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(int categoryId)
        {
            try
            {
                var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    return NotFound(new { message = "Категория не найдена" });
                }

                var products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == categoryId);

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении продуктов категории {CategoryId}", categoryId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // POST запрос (создание с возможностью передачи ID)
        // ========================================

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == createDto.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Категория не найдена" });
                }

                if (createDto.Id.HasValue)
                {
                    var existingProduct = await _unitOfWork.Products.GetByIdAsync(createDto.Id.Value);
                    if (existingProduct != null)
                    {
                        return BadRequest(new { message = $"Продукт с ID {createDto.Id.Value} уже существует" });
                    }
                }

                var product = new Product
                {
                    Id = createDto.Id ?? 0,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Price = createDto.Price,
                    CategoryId = createDto.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = product.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании продукта");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // PUT запрос (обновление)
        // ========================================

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Продукт не найден" });
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    existingProduct.Name = updateDto.Name;
                }

                if (updateDto.Description != null)
                {
                    existingProduct.Description = updateDto.Description;
                }

                if (updateDto.Price.HasValue)
                {
                    existingProduct.Price = updateDto.Price.Value;
                }

                if (updateDto.CategoryId.HasValue)
                {
                    var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == updateDto.CategoryId);
                    if (!categoryExists)
                    {
                        return BadRequest(new { message = "Категория не найдена" });
                    }
                    existingProduct.CategoryId = updateDto.CategoryId.Value;
                }

                existingProduct.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Products.Update(existingProduct);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении продукта с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // DELETE запрос
        // ========================================

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Продукт не найден" });
                }

                _unitOfWork.Products.Delete(product);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении продукта с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}