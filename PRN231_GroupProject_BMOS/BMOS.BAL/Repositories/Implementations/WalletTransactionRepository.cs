using AutoMapper;
using BMOS.BAL.DTOs.WalletTransactions;
using BMOS.BAL.DTOs.WalletTransactions.Momo;
using BMOS.BAL.DTOs.WalletTransactions.Zalopay;
using BMOS.BAL.Errors;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Quartz.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ZaloPay.Helper;
using ZaloPay.Helper.Crypto;

namespace BMOS.BAL.Repositories.Implementations
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public WalletTransactionRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region Create wallet transaction
        public async Task<GetWalletTransactionResponse> CreateWalletTransactionAsync(PostWalletTransactionRequest model, MomoConfigModel _config)
        {
            try
            {
                #region Validation
                var account = await _unitOfWork.AccountDAO.GetAccountByEmail(model.Email);
                if (account == null)
                {
                    throw new NotFoundException("Account does not exist.");
                }
                #endregion

                #region Add transaction to Db (Status: Pending)
                // Must save to database to check Amount and [Currency = false] when there is a notification from Momo
                var orderId = DateTime.Now.Ticks.ToString();
                var wallet = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync(account.ID);

                WalletTransaction transaction = new WalletTransaction
                {
                    RechargeID = orderId,
                    RechargeTime = DateTime.Now,
                    Amount = model.Amount,
                    Content = "Deposit money into BMOS wallet through MOMO.",
                    TransactionType = WalletTransactionEnum.TransactionType.DEPOSIT.ToString(),
                    RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.PENDING,
                    Wallet = wallet!
                };

                await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(transaction);
                await _unitOfWork.CommitAsync();
                #endregion

                #region Send request to momo
                var requestId = orderId + "id";
                var rawData = $"accessKey={_config.AccessKey}&amount={model.Amount}&extraData={_config.ExtraData}&ipnUrl={_config.NotifyUrl}&orderId={orderId}&orderInfo={_config.OrderInfo}&partnerCode={_config.PartnerCode}&redirectUrl={model.RedirectUrl}&requestId={requestId}&requestType={_config.RequestType}";
                var signature = EncodeHelper.ComputeHmacSha256(rawData, _config.SecretKey!);

                var client = new RestClient(_config.PayGate! + "/create");
                var request = new RestRequest() { Method = Method.Post };
                request.AddHeader("Content-Type", "application/json; charset=UTF-8");

                // Body of request
                PostTransactionMomoRequest bodyContent = new PostTransactionMomoRequest
                {
                    partnerCode = _config.PartnerCode,
                    partnerName = _config.PartnerName,
                    storeId = _config.PartnerCode,
                    requestType = _config.RequestType,
                    ipnUrl = _config.NotifyUrl,
                    redirectUrl = model.RedirectUrl,
                    orderId = orderId,
                    amount = model.Amount,
                    lang = _config.Lang,
                    autoCapture = _config.AutoCapture,
                    orderInfo = _config.OrderInfo,
                    requestId = requestId,
                    extraData = _config.ExtraData,
                    orderExpireTime = _config.OrderExpireTime,
                    signature = signature
                };

                request.AddParameter("application/json", JsonConvert.SerializeObject(bodyContent), ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var responseContent = JsonConvert.DeserializeObject<PostTransactionMomoResponse>(response.Content!);
                    var walletResponse = _mapper.Map<GetWalletTransactionResponse>(transaction);
                    walletResponse.PayUrl = responseContent!.PayUrl;
                    walletResponse.Deeplink = responseContent!.Deeplink;
                    walletResponse.QrCodeUrl = responseContent!.QrCodeUrl;
                    walletResponse.Applink = responseContent.Applink;

                    return walletResponse;
                }

                throw new Exception("No server is available to handle this request.");
               
                #endregion

            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Create zalo transaction
        public async Task<PostTransactionZaloResponse> CreateZaloTransactionAsync(PostWalletTransactionRequest model, ZaloConfigModel config)
        {
            #region Validation
            var account = await _unitOfWork.AccountDAO.GetAccountByEmail(model.Email);
            if (account == null)
            {
                throw new NotFoundException("Account does not exist.");
            }
            #endregion

            #region Add transaction to Db (Status: Pending)
            // Must save to database to check Amount and [Currency = false] when there is a notification from Momo
            var orderId = DateTime.Now.Ticks.ToString();
            var wallet = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync(account.ID);

            WalletTransaction transaction = new WalletTransaction
            {
                RechargeID = orderId,
                RechargeTime = DateTime.Now,
                Amount = model.Amount,
                Content = "Deposit money into BMOS wallet through ZaloPay.",
                TransactionType = WalletTransactionEnum.TransactionType.DEPOSIT.ToString(),
                RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.PENDING,
                Wallet = wallet!
            };

            await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(transaction);
            await _unitOfWork.CommitAsync();
            #endregion

            #region Send request to Zalopay
            Random rnd = new Random();
            var embed_data = new Dictionary<string, string>(); // redirecturl here
            embed_data.Add("redirecturl", model.RedirectUrl);

            var items = new[] { new { } };
            var app_trans_id = rnd.Next(1000000); // Generate a random order's ID.
            var param = new Dictionary<string, string>();

            param.Add("app_id", config.app_id!);
            param.Add("app_user", config.app_user!);
            param.Add("app_time", Utils.GetTimeStamp().ToString());
            param.Add("amount", model.Amount.ToString());
            param.Add("app_trans_id", DateTime.Now.ToString("yyMMdd") + "_" + app_trans_id); // mã giao dich có định dạng yyMMdd_xxxx
            param.Add("embed_data", JsonConvert.SerializeObject(embed_data));
            param.Add("item", JsonConvert.SerializeObject(items));
            param.Add("description", "Lazada - Thanh toán đơn hàng #" + app_trans_id);
            param.Add("bank_code", config.bank_code!);

            var data = config.app_id! + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|"
              + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, config.key!, data));

            var result = await HttpHelper.PostFormAsync(config.url!, param);
            var response = PostTransactionZaloResponse.FromDictionary(result);
 
            return response;
            #endregion
        }
        #endregion

        #region Listen notification from Momo
        public async Task<GetWalletTransactionResponse> PaymentNotificationAsync(string id, MomoConfigModel _config)
        {
            try
            {
                #region Validation

                var transaction = await _unitOfWork.WalletTransactionDAO.GetWalletTransactionByRechargeIDAsync(id);
                if (transaction == null)
                {
                    throw new NotFoundException("Transaction does not exist.");
                }

                if (transaction.RechargeStatus == (int)WalletTransactionEnum.RechangeStatus.SUCCESSED ||
                    transaction.RechargeStatus == (int)WalletTransactionEnum.RechangeStatus.FAILED)
                {
                    throw new BadRequestException("This transaction has been processed.");
                }

                #region Query transaction
                var requestId = transaction.RechargeID + "id";
                var rawData = $"accessKey={_config.AccessKey}&orderId={transaction.RechargeID}&partnerCode={_config.PartnerCode}&requestId={requestId}";
                var signature = EncodeHelper.ComputeHmacSha256(rawData, _config.SecretKey!);

                var client = new RestClient(_config.PayGate! + "/query");
                var request = new RestRequest() { Method = Method.Post };
                request.AddHeader("Content-Type", "application/json; charset=UTF-8");

                QueryTransactionMomoRequest queryTransaction = new QueryTransactionMomoRequest
                {
                    partnerCode = _config.PartnerCode,
                    requestId = requestId,
                    orderId = transaction.RechargeID,
                    lang = _config.Lang,
                    signature = signature

                };

                request.AddParameter("application/json", JsonConvert.SerializeObject(queryTransaction), ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                var responseResult = JsonConvert.DeserializeObject<QueryTransactionMomoResponse>(response.Content!);
                #endregion

                // Check Amount and [Currency = fasle] 
                if (responseResult!.Amount != transaction.Amount)
                {
                    throw new BadRequestException("Amount of transaction and notification does not matched!");
                }

                // Check legit of signature - coming soon
                #endregion

                #region Update wallettransaction and wallet (if success)
                // ResultCode = 0: giao dịch thành công
                // ResultCode = 9000: giao dịch được cấp quyền (authorization) thành công
                if (responseResult.ResultCode == 0 || responseResult.ResultCode == 9000)
                {
                    transaction.RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED;
                    _unitOfWork.WalletTransactionDAO.UpdateWalletTransaction(transaction);

                    // If amount = null, amount = default value of type
                    transaction.Wallet.Balance += responseResult.Amount.GetValueOrDefault(0m);
                    _unitOfWork.WalletDAO.UpdateWallet(transaction.Wallet);
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<GetWalletTransactionResponse>(transaction);
                }
                else if (responseResult.ResultCode == 1000)
                {
                    throw new BadRequestException("Transaction is initiated, waiting for user confirmation!");
                }
                else
                {
                    transaction.RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.FAILED;
                    _unitOfWork.WalletTransactionDAO.UpdateWalletTransaction(transaction);
                    await _unitOfWork.CommitAsync();

                    throw new BadRequestException("Recharge failed!");
                }
                #endregion
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Process wallet transaction
        public async Task ProcessWalletTransaction()
        {
            try
            {
                var walletTransactions = await _unitOfWork.WalletTransactionDAO.GetWalletTransactionsPendingAsync();
                if (walletTransactions == null || walletTransactions.Count() == 0)
                {
                    return;
                }

                foreach (var transaction in walletTransactions)
                {
                    if (DateTime.Now > transaction!.RechargeTime.AddMinutes(2))
                    {
                        transaction.RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.FAILED;
                        _unitOfWork.WalletTransactionDAO.UpdateWalletTransaction(transaction);
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion
    }
}
