using BMOS.BAL.DTOs.Accounts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Accounts
{
    public class PostAccountValidation : AbstractValidator<PostAccountRequest>
    {
        public PostAccountValidation()
        {
            #region Email
            RuleFor(a => a.Email)
               .NotNull().WithMessage("{PropertyName} is null.")
               .NotEmpty().WithMessage("{PropertyName} is empty.")
               .EmailAddress().WithMessage("{PropertyName} is invalid.")
               .Length(10, 50).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Password
            RuleFor(a => a.PasswordHash)
              .NotNull().WithMessage("{PropertyName} is null.")
              .NotEmpty().WithMessage("{PropertyName} is empty.")
              .Length(8, 20).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion
        }
    }
}
