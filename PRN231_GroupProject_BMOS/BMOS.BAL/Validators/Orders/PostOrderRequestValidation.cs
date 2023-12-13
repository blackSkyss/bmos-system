using BMOS.BAL.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Orders
{
    public class PostOrderRequestValidation : AbstractValidator<PostOrderRequest>
    {
        public PostOrderRequestValidation()
        {

            #region Email
            RuleFor(p => p.Email)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .EmailAddress().WithMessage("{PropertyName} is invalid email type.");
            #endregion

            #region Meals
            RuleForEach(p => p.Meals)
                .SetValidator(new PostMealOrderRequestValidation());
            #endregion
        }
    }
}
