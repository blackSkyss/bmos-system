using BMOS.BAL.DTOs.Accounts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Accounts
{
    public class ChangePasswordValidation : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidation()
        {
            #region OldPassWord
            RuleFor(c => c.OldPassWord)
                 .Cascade(CascadeMode.Continue)
                 .NotEmpty().WithMessage("{PropertyName} is empty!")
                 .NotNull().WithMessage("{PropertyName} is null!")
                 .Length(8, 30).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters!");
            #endregion

            #region NewPassWord
            RuleFor(c => c.NewPassWord)
                 .Cascade(CascadeMode.Continue)
                 .NotEmpty().WithMessage("{PropertyName} is empty!")
                 .NotNull().WithMessage("{PropertyName} is null!")
                 .Length(8, 30).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters!");
            #endregion

            #region ConFirmPassword
            RuleFor(c => c.ConFirmPassword)
                 .Cascade(CascadeMode.Continue)
                 .NotEmpty().WithMessage("{PropertyName} is empty!")
                 .NotNull().WithMessage("{PropertyName} is null!")
                 .Length(8, 30).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters!");
            #endregion
        }
    }
}
