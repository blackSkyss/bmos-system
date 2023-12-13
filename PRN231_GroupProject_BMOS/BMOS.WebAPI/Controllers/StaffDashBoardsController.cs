using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs;
using BMOS.BAL.DTOs.DashBoard;
using BMOS.BAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace BMOS.WebAPI.Controllers
{

    public class StaffDashBoardsController : ODataController
    {
        private IDashBoardRepository _dashBoardRepository;
        public StaffDashBoardsController(IDashBoardRepository dashBoardRepository)
        {
            this._dashBoardRepository = dashBoardRepository;
        }

        #region Get Dash Board Staff
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get()
        {
            var staffDashBoard = await this._dashBoardRepository.GetStaffDashBoardAsync();
            return Ok(staffDashBoard);
        }
        #endregion
    }
}
