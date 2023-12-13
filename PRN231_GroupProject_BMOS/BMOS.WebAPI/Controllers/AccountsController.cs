using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace BMOS.WebAPI.Controllers.Customer
{

    public class AccountsController : ODataController
    {
        private IAccountRepository _accountRepository;
        private IValidator<ChangePasswordRequest> _changePasswordValidator;
        public AccountsController(
            IAccountRepository accountRepository,
            IValidator<ChangePasswordRequest> changePasswordValidator
            )
        {
            _accountRepository = accountRepository;
            _changePasswordValidator = changePasswordValidator;
        }

        [EnableQuery]
        public async Task<IActionResult> Put([FromRoute] int key, [FromBody] ChangePasswordRequest changePasswordRequest)
        {
            ValidationResult validationResult = await _changePasswordValidator.ValidateAsync(changePasswordRequest);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetAccountResponse result = await this._accountRepository.ChangPassword(key, changePasswordRequest);
            return Updated(result);
        }
    }
}
