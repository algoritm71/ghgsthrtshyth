using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data.Interfaces;
using WebApplication2.Models;
using WebApplication2.DTOs;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ========================================
        // GET запросы
        // ========================================

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();

                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                });

                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка категорий");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = "Категория не найдена" });
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreatedAt = category.CreatedAt
                };

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении категории с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/categories/5/products
        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Категория не найдена" });
                }

                var products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == id);

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
                _logger.LogError(ex, "Ошибка при получении продуктов категории {CategoryId}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // POST запрос (создание с возможностью передачи ID)
        // ========================================

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (createDto.Id.HasValue)
                {
                    var existingCategory = await _unitOfWork.Categories.GetByIdAsync(createDto.Id.Value);
                    if (existingCategory != null)
                    {
                        return BadRequest(new { message = $"Категория с ID {createDto.Id.Value} уже существует" });
                    }
                }

                var category = new Category
                {
                    Id = createDto.Id ?? 0,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreatedAt = category.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // PUT запрос (обновление)
        // ========================================

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { message = "Категория не найдена" });
                }

                existingCategory.Name = updateDto.Name;
                existingCategory.Description = updateDto.Description;

                _unitOfWork.Categories.Update(existingCategory);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении категории с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // DELETE запрос
        // ========================================

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Категория не найдена" });
                }

                var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    return BadRequest(new { message = "Нельзя удалить категорию, содержащую продукты" });
                }

                _unitOfWork.Categories.Delete(category);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении категории с Id={Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}