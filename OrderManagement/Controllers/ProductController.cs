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

            _service.Create(dto);
            return Ok("Product created.");
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, ProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = _service.Get(id);
            if (existing == null)
                return NotFound();

            _service.Update(id, dto);
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
