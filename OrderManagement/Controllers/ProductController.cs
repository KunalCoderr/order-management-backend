using OrderManagement.DTOsModels;
using OrderManagement.Filters;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            // Manually instantiate required dependencies
            var dbContext = new OrderManagementEntities(); // your EF context
            IProductRepository productRepository = new ProductRepository(dbContext);
            _service = service;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var products = _service.GetAll();
            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var product = _service.Get(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(ProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Name))
                return BadRequest("Product name is required.");

            // Format product name to Title Case using CommonUtils
            dto.Name = CommonUtils.CommonUtils.ToTitleCase(dto.Name);

            // Optional: Log product creation request
            CommonUtils.CommonUtils.LogMessage($"Creating product: Name='{dto.Name}', Price={dto.Price}");

            _service.Create(dto);

            // Log success
            CommonUtils.CommonUtils.LogMessage($"Product '{dto.Name}' created successfully.");
            return Ok("Product created.");
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, ProductDTO dto)
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

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var existing = _service.Get(id);
            if (existing == null)
                return NotFound();

            _service.Delete(id);
            return Ok("Product deleted.");
        }
    }
}
