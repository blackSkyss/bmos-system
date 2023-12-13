using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.DashBoard;
using BMOS.BAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace BMOS.WebAPI.Controllers
{
    public class StoreOwnerDashBoardsController : ODataController
    {
        private IDashBoardRepository _dashBoardRepository;
        public StoreOwnerDashBoardsController(IDashBoardRepository dashBoardRepository)
        {
            this._dashBoardRepository = dashBoardRepository;
        }

        [EnableQuery]
        [PermissionAuthorize("Store Owner")]
        public async Task<IActionResult> Get([FromQuery] int? year)
        {
            GetStoreOwnerDashBoardResponse storeOwnerDashBoard = await this._dashBoardRepository.GetStoreOwnerDashBoardAsync(year);
            return Ok(storeOwnerDashBoard);
        }
    }
}
