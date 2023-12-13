using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.DTOs.WalletTransactions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Orders
{
    public class PostMealOrderRequestValidation : AbstractValidator<PostListMealOrderRequest>
    {
        public PostMealOrderRequestValidation() {

            #region Id
            RuleFor(w => w.Id)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.");
            #endregion

            #region Amount
            RuleFor(w => w.Id)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .InclusiveBetween(1, 100).WithMessage("{PropertyName} min is 1 and max is 100.");
            #endregion
        }
    }
}
