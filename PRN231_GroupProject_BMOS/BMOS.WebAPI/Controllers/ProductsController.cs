using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.BAL.Validators.Products;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;

namespace BMOS.WebAPI.Controllers
{
    public class ProductsController : ODataController
    {

        IProductRepository _productRepository;
        IValidator<PostProductRequest> _postProductValidator;
        IValidator<UpdateProductRequest> _updateProductValidator;
        IOptions<FireBaseImage> _firebaseImageOptions;
        public ProductsController(IProductRepository productRepository,
            IValidator<PostProductRequest> postProductValidator,
            IOptions<FireBaseImage> firebaseImageOptions,
            IValidator<UpdateProductRequest> updateProductValidator)
            

        {
            this._updateProductValidator = updateProductValidator;
            this._productRepository = productRepository;
            this._postProductValidator = postProductValidator;
            this._firebaseImageOptions = firebaseImageOptions;
        }

        #region Create Product
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Post([FromForm] PostProductRequest postProductRequest)
        {
            var resultValid = await _postProductValidator.ValidateAsync(postProductRequest);
            if (!resultValid.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(resultValid);
                throw new BadRequestException(error);
            }
            GetProductResponse product = await this._productRepository.CreateNewProductAsync(postProductRequest, _firebaseImageOptions.Value, HttpContext);
            return Created(product);
        }
        #endregion

        #region Update Product
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Put([FromRoute] int key, [FromForm] UpdateProductRequest updateProductRequest)
        {
            var resultValid = _updateProductValidator.Validate(updateProductRequest);
            if (!resultValid.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(resultValid);
                throw new BadRequestException(error);
            }
            GetProductResponse product = await this._productRepository.UpdateProductAsync(key, updateProductRequest, _firebaseImageOptions.Value, HttpContext);
            return Updated(product);
        }
        #endregion

        #region Get Products
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get()
        {
            List<GetProductResponse> products = await this._productRepository.GetProductsAsync();
            return Ok(products);
        }
        #endregion

        [HttpGet("odata/Products/Active/Product")]
        [EnableQuery]
        public async Task<IActionResult> ActiveProducts()
        {
            List<GetProductResponse> products = await this._productRepository.GetActiveProducts();
            return Ok(products);
        }
        
        [HttpGet("odata/Products/Active/Product/{productId}")]
        [EnableQuery(MaxExpansionDepth =3)]
        public async Task<IActionResult> ActiveProduct(int productId)
        {
            GetProductResponse product = await this._productRepository.GetActiveProduct(productId);
            return Ok(product);
        }

        #region Get Product Detail By Id
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            GetProductResponse product = await this._productRepository.GetProductDetailByIdAsync(key);
            return Ok(product);
        }
        #endregion

        #region Delete Product 
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Delete([FromRoute] int key)
        {
            await this._productRepository.DeleteProductAsync(key, this.HttpContext);
            return NoContent();
        }
        #endregion
    }
}
