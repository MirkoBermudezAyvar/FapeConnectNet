// Core/Services/ProductService.cs
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;
using venar_bus_api_jakar_bckd_net.DTOs;

namespace venar_bus_api_jakar_bckd_net.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;

        public ProductService(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.FindAsync(p => p.CategoryId == categoryId);
        }

        public async Task<Product> CreateProductAsync(CreateProductDto productDto)
        {
            // Verify category exists
            var categoryExists = await _categoryRepository.ExistsAsync(productDto.CategoryId);
            if (!categoryExists)
            {
                throw new KeyNotFoundException($"Category with id {productDto.CategoryId} not found.");
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageUrl = productDto.ImageUrl,
                SKU = productDto.SKU,
                CategoryId = productDto.CategoryId
            };

            return await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }

            // Update properties if they're provided
            if (!string.IsNullOrEmpty(productDto.Name))
                product.Name = productDto.Name;

            if (!string.IsNullOrEmpty(productDto.Description))
                product.Description = productDto.Description;

            if (productDto.Price.HasValue)
                product.Price = productDto.Price.Value;

            if (productDto.Stock.HasValue)
                product.Stock = productDto.Stock.Value;

            if (productDto.ImageUrl != null)
                product.ImageUrl = productDto.ImageUrl;

            if (!string.IsNullOrEmpty(productDto.SKU))
                product.SKU = productDto.SKU;

            if (productDto.CategoryId.HasValue)
            {
                // Verify new category exists
                var categoryExists = await _categoryRepository.ExistsAsync(productDto.CategoryId.Value);
                if (!categoryExists)
                {
                    throw new KeyNotFoundException($"Category with id {productDto.CategoryId.Value} not found.");
                }
                product.CategoryId = productDto.CategoryId.Value;
            }

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string term)
        {
            term = term.ToLower();
            return await _productRepository.FindAsync(p => 
                p.Name.ToLower().Contains(term) || 
                p.Description.ToLower().Contains(term) || 
                p.SKU.ToLower().Contains(term));
        }

        public async Task UpdateStockAsync(int id, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }

            product.Stock = quantity;
            await _productRepository.UpdateAsync(product);
        }
    }
}