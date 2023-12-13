using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.DTOs.WalletTransactions;
using BMOS.BAL.DTOs.WalletTransactions.Momo;
using BMOS.BAL.DTOs.WalletTransactions.Zalopay;
using BMOS.BAL.Errors;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.BAL.Validators.WalletTransactions;
using BMOS.DAL.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace BMOS.WebAPI.Controllers
{
    public class WalletTransactionsController : ODataController
    {
        private readonly IOptions<MomoConfigModel> _optionsMomo;
        private readonly IOptions<ZaloConfigModel> _optionsZalopay;
        private IWalletTransactionRepository _walletTransactionRepository;
        private IValidator<PostWalletTransactionRequest> _postWalletTransactionValidator;
        public WalletTransactionsController(IOptions<MomoConfigModel> optionsMomo,
                                            IOptions<ZaloConfigModel> optionsZalopay,
                                            IWalletTransactionRepository walletTransactionRepository,
                                            IValidator<PostWalletTransactionRequest> postWalletTransactionValidator
                                            )
        {
            _optionsMomo = optionsMomo;
            _optionsZalopay = optionsZalopay;
            _walletTransactionRepository = walletTransactionRepository;
            _postWalletTransactionValidator = postWalletTransactionValidator;
        }

        #region Creat wallet transaction(Momo)
        [HttpPost("odata/WalletTransactions/CreateMomoTransaction")]
        [EnableQuery]
        //[PermissionAuthorize("Customer")]
        public async Task<IActionResult> PostMomo([FromBody] PostWalletTransactionRequest request)
        {
            var validationResult = await _postWalletTransactionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            var result = await this._walletTransactionRepository.CreateWalletTransactionAsync(request, _optionsMomo.Value);
            return Ok(result);
        }
        #endregion

        #region Creat wallet transaction(Zalopay)
        [HttpPost("odata/WalletTransactions/CreateZalopayTransaction")]
        [EnableQuery]
        //[PermissionAuthorize("Customer")]
        public async Task<IActionResult> PostZalopay([FromBody] PostWalletTransactionRequest request)
        {
            var validationResult = await _postWalletTransactionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            var result = await this._walletTransactionRepository.CreateZaloTransactionAsync(request, _optionsZalopay.Value);
            return Ok(result);
        }
        #endregion

        #region IPN MOMO || Query transaction.then(Update transaction)
        [EnableQuery]
        [PermissionAuthorize("Customer")]
        public async Task<IActionResult> Put([FromRoute] string key)
        {
            var result = await _walletTransactionRepository.PaymentNotificationAsync(key, _optionsMomo.Value);
            return Updated(result);
        }
        #endregion

        #region IPN Zalopay(comming soon)
        #endregion
    }
}
