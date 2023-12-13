using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.DTOs.Wallets;
using BMOS.BAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace BMOS.WebAPI.Controllers
{
    public class WalletsController : ODataController
    {
        private readonly IWalletRepository _walletRepository;

        public WalletsController(IWalletRepository walletRepository)
        {
            this._walletRepository = walletRepository;
        }

        [EnableQuery]
        [PermissionAuthorize("Store Owner", "Customer")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            GetWalletsResponse wallet = await this._walletRepository.GetWalletByUserID(key);
            return Ok(wallet);
        }
    }
}
