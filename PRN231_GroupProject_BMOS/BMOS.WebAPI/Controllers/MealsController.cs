using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;

namespace BMOS.WebAPI.Controllers
{

    public class MealsController : ODataController
    {
        private readonly IMealRepository _mealRepository;
        private IOptions<FireBaseImage> _firebaseImageOptions;
        private IValidator<PostMealRequest> _postMealValidator;
        private IValidator<UpdateMealRequest> _updateMealValidator;

        public MealsController(IMealRepository mealRepository, IOptions<FireBaseImage> firebaseImageOptions,
            IValidator<PostMealRequest> postMealValidator, IValidator<UpdateMealRequest> updateMealValidator)
        {
            this._mealRepository = mealRepository;
            this._firebaseImageOptions = firebaseImageOptions;
            this._postMealValidator = postMealValidator;
            this._updateMealValidator = updateMealValidator;
        }

        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get()
        {
            List<GetMealResponse> meals = await this._mealRepository.GetMealsAsync();
            return Ok(meals);
        }

        [HttpGet("odata/Meals/Active/Meal")]
        [EnableQuery]
        public async Task<IActionResult> GetActiveMeals()
        {
            List<GetMealResponse> meals = await this._mealRepository.GetAvailableMeals();
            return Ok(meals);
        }
        
        [HttpGet("odata/Meals/Active/Meal/{mealID}")]
        [EnableQuery(MaxExpansionDepth=3)]
        public async Task<IActionResult> GetActiveMeal(int mealID)
        {
            GetMealResponse meal = await this._mealRepository.GetAvailableMeal(mealID);
            return Ok(meal);
        }

        [EnableQuery(MaxExpansionDepth=4)]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            GetMealResponse meal = await this._mealRepository.GetMealByID(key);
            return Ok(meal);
        }

        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Post([FromForm] PostMealRequest meal)
        {
            ValidationResult validationResult = await this._postMealValidator.ValidateAsync(meal);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetMealResponse getMealResponse = await this._mealRepository.CreateMeal(_firebaseImageOptions.Value, meal, this.HttpContext);
            return Created(getMealResponse);
        }

        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Put([FromRoute] int key, [FromForm] UpdateMealRequest meal)
        {
            ValidationResult validationResult = await this._updateMealValidator.ValidateAsync(meal);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }
            GetMealResponse getMealResponse = await this._mealRepository.UpdateMeal(key, _firebaseImageOptions.Value, meal, this.HttpContext);
            return Updated(getMealResponse);
        }

        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Delete([FromRoute] int key)
        {
            await this._mealRepository.DeleteMeal(key, this.HttpContext);
            return NoContent();
        }

    }
}
