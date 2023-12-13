using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.BAL.Validators.Customers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;

namespace BMOS.WebAPI.Controllers
{

    public class CustomersController : ODataController
    {

        private ICustomerRepository _customerRepository;
        private IValidator<UpdateCustomerRequest> _updateCustomerValidator;
        private IValidator<RegisterRequest> _registerValidator;
        private IOptions<FireBaseImage> _firebaseImageOptions;
        public CustomersController(ICustomerRepository customerRepository,
            IValidator<UpdateCustomerRequest> updateCustomerValidator,
            IValidator<RegisterRequest> registerValidator,
            IOptions<FireBaseImage> firebaseImageOptions)
        {
            this._customerRepository = customerRepository;
            this._updateCustomerValidator = updateCustomerValidator;
            this._registerValidator = registerValidator;
            this._firebaseImageOptions = firebaseImageOptions;
        }

        #region View Profile By AccountId
        [EnableQuery]
        [PermissionAuthorize("Customer", "Store Owner")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            GetCustomerResponse customer = await this._customerRepository.GetCustomerByAccountIdAsync(key);
            return Ok(customer);
        }
        #endregion

        #region Update Customer
        [EnableQuery]
        [PermissionAuthorize("Customer", "Store Owner")]
        public async Task<IActionResult> Put([FromRoute] int key, [FromForm] UpdateCustomerRequest updateCustomerRequest)
        {
            ValidationResult validationResult = await _updateCustomerValidator.ValidateAsync(updateCustomerRequest);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetCustomerResponse customer = await this._customerRepository.UpdateCustomerProfileByAccountIdAsync(key,
                                                                                                                _firebaseImageOptions.Value,
                                                                                                                updateCustomerRequest);
            return Updated(customer);
        }

        //get all customer
        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Get()
        {
            List<GetCustomerResponse> customers = await this._customerRepository.GetCustomersAsync();
            return Ok(customers);
        }

        //ban customer
        [EnableQuery]
        [HttpPut("odata/Customers/{key}/Ban")]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> BanCustomer([FromRoute] int key)
        {
            GetCustomerResponse customer = await this._customerRepository.BanCustomerAsync(key);
            return Ok(customer);
        }
        #endregion

        #region Register
        [HttpPost("odata/Customers/Register")]
        [EnableQuery]
        public async Task<IActionResult> Post([FromForm] RegisterRequest registerRequest)
        {
            ValidationResult validationResult = await _registerValidator.ValidateAsync(registerRequest);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetCustomerResponse customer = await this._customerRepository
                .Register(_firebaseImageOptions.Value, registerRequest);
            return Ok();
        }
        #endregion
    }
}
