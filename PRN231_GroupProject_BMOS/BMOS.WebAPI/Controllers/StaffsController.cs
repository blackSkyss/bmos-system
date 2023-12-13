using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.DTOs.Staffs;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;

namespace BMOS.WebAPI.Controllers
{
    public class StaffsController : ODataController
    {
        private IStaffRepository _staffRepository;
        private IValidator<PostStaffRequest> _postStaffValidator;
        private IValidator<UpdateStaffRequest> _putStaffValidator;
        private IOptions<FireBaseImage> _firebaseImageOptions;
        public StaffsController(IStaffRepository staffRepository,
                                IValidator<PostStaffRequest> postStaffValidator,
                                IOptions<FireBaseImage> firebaseImageOptions,
                                IValidator<UpdateStaffRequest> putStaffValidator)
        {
            _staffRepository = staffRepository;
            _postStaffValidator = postStaffValidator;
            _firebaseImageOptions = firebaseImageOptions;
            _putStaffValidator = putStaffValidator;
        }

        #region Get staffs
        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Get()
        {
            var result = await _staffRepository.GetStaffsAsync();
            return Ok(result);
        }
        #endregion

        #region Get staff detail
        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            var result = await _staffRepository.GetStaffDetailAsync(key);
            return Ok(result);
        }
        #endregion

        #region Create staff
        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Post([FromForm] PostStaffRequest request)
        {
            var validationResult = await _postStaffValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            var result = await _staffRepository.CreateStaffAsync(request, _firebaseImageOptions.Value);
            return Created(result);
        }
        #endregion

        #region Delete staff
        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Delete([FromRoute] int key)
        {
            await _staffRepository.DeteleStaffAsync(key);
            return NoContent();
        }
        #endregion

        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Put([FromRoute] int key, [FromForm] UpdateStaffRequest updateStaffRequest)
        {
            ValidationResult validationResult = await this._putStaffValidator.ValidateAsync(updateStaffRequest);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetStaffResponse getStaffResponse = await this._staffRepository.UpdateStaffAsync(key, updateStaffRequest, this._firebaseImageOptions.Value);
            return Updated(getStaffResponse);
        }
    }
}
