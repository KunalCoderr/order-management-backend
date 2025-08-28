using OrderManagement.DTOsModels;
using OrderManagement.Filters;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Web.Http;

namespace OrderManagement.Controllers
{
    [AuthorizeSession]
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            var dbContext = new OrderManagementEntities();
            IProductRepository productRepository = new ProductRepository(dbContext);
            _service = service;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            try
            {
                var products = _service.GetAll();
                return Ok(products);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in GetAll Products: {ex.Message}\n{ex.StackTrace}");

                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var product = _service.Get(id);

                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in product by Id Get({id} : {ex.Message}\n{ex.StackTrace}");

                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(ProductDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Name))
                    return BadRequest("Product name is required.");

                // Format product name to Title Case using CommonUtils
                dto.Name = CommonUtils.CommonUtils.ToTitleCase(dto.Name);

                CommonUtils.CommonUtils.LogMessage($"Creating product: Name='{dto.Name}', Price={dto.Price}");

                _service.Create(dto);

                CommonUtils.CommonUtils.LogMessage($"Product '{dto.Name}' created successfully.");
                return Ok("Product created.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error creating product '{dto?.Name}': {ex.Message}\n{ex.StackTrace}");

                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, ProductDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate incoming data
                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Name))
                    return BadRequest("Product name is required.");

                dto.Name = CommonUtils.CommonUtils.ToTitleCase(dto.Name);

                var existing = _service.Get(id);
                if (existing == null)
                    return NotFound();

                CommonUtils.CommonUtils.LogMessage($"Updating product id={id}: New Name='{dto.Name}', Price={dto.Price}");

                _service.Update(id, dto);

                CommonUtils.CommonUtils.LogMessage($"Product id={id} updated successfully.");

                return Ok("Product updated.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error updating product id={id}: {ex.Message}\n{ex.StackTrace}");
                return InternalServerError();
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var existing = _service.Get(id);
                if (existing == null)
                    return NotFound();

                _service.Delete(id);
                return Ok("Product deleted.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error deleting product id={id}: {ex.Message}\n{ex.StackTrace}");
                return InternalServerError();
            }
        }
    }
}
