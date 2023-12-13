using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.Helpers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Customers
{
    public class CustomerValidation : AbstractValidator<UpdateCustomerRequest>
    {
        private const int MAX_BYTES = 2048000;
        public CustomerValidation()
        {
            #region FullName
            RuleFor(c => c.FullName)
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .Length(5, 80).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Address
            RuleFor(c => c.Address)
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .Length(5, 120).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Phone
            RuleFor(c => c.Phone)
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .Length(10).WithMessage("Phone must be 10 characters.");
            #endregion

            #region Gender
            RuleFor(c => c.Gender)
                .Must(x => x == true || x == false).WithMessage("{PropertyName} must be either true or false.");
            #endregion

            #region PasswordHash
            RuleFor(c => c.PasswordHash).Custom((password, context) =>
            {
                if(password != null && password.Length >= 8 && password.Length <= 30)
                {
                    context.AddFailure($"Password is required from 8 to 30 characters.");
                }
            });
            #endregion

            #region BirthDate
            RuleFor(c => c.BirthDate)
                   .NotEmpty().WithMessage("{PropertyName} is empty.")
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .Must(date => DateHelper.IsValidBirthday(date)).WithMessage("{PropertyName} must greater than 13 year old.")
                   .Must(date => DateHelper.IsValidBirthday(date)).WithMessage("{PropertyName} must greater than 13 year old.");
            #endregion

            #region Avatar
            RuleFor(p => p.Avatar)
                   .ChildRules(pro => pro.RuleFor(img => img.Length).ExclusiveBetween(0, MAX_BYTES)
                   .WithMessage($"Avatar is required file length greater than 0 and less than {MAX_BYTES / 1024 / 1024} MB"));
            RuleFor(p => p.Avatar)
                   .ChildRules(pro => pro.RuleFor(img => img.FileName).Must(FileHelper.HaveSupportedFileType).WithMessage("Avatar is required extension type .png, .jpg, .jpeg"));
            #endregion
        }


    }
}
