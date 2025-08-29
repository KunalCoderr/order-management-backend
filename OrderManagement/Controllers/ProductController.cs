using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OrderManagement.Controllers
{
    [ApiController]
    [Route("api/product")]
    [Authorize]
    public class ProductController : BaseApiController
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public IActionResult GetAll()
        {
            try
            {
                var products = _service.GetAll();

                return Success(products, "Products retrieved successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in GetAll Products: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while retrieving products.");
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            try
            {
                var product = _service.Get(id);

                if (product == null)
                    return Fail<object>("Product not found.");

                return Success(product, "Product retrieved successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in Get product by Id {id} : {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while retrieving the product.");
            }
        }

        [HttpPost("")]
        public IActionResult Create(ProductDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Fail<object>("Invalid product data.");

                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Name) || !CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Price.ToString()))
                    return Fail<object>("Product name and valid price are required.");

                // Format product name to Title Case using CommonUtils
                dto.Name = CommonUtils.CommonUtils.ToTitleCase(dto.Name);

                CommonUtils.CommonUtils.LogMessage($"Creating product: Name='{dto.Name}', Price={dto.Price}");

                _service.Create(dto);

                CommonUtils.CommonUtils.LogMessage($"Product '{dto.Name}' created successfully.");
                return Success("", "Product created successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error creating product '{dto?.Name}': {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while creating the product.");
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, ProductDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Fail<object>("Invalid product data.");

                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Name) || !CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Price.ToString()))
                    return Fail<object>("Product name and valid price are required.");

                dto.Name = CommonUtils.CommonUtils.ToTitleCase(dto.Name);

                var existing = _service.Get(id);
                if (existing == null)
                    return NotFound();

                CommonUtils.CommonUtils.LogMessage($"Updating product id={id}: New Name='{dto.Name}', Price={dto.Price}");

                _service.Update(id, dto);

                CommonUtils.CommonUtils.LogMessage($"Product id={id} updated successfully.");

                return Success<object>(null, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error updating product id={id}: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while updating the product.");
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existing = _service.Get(id);
                if (existing == null)
                    return Fail<object>("Product not found.");

                _service.Delete(id);
                return Success<object>(null, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error deleting product id={id}: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while deleting the product.");
            }
        }
    }
}
