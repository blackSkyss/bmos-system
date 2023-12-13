using BMOS.BAL.DTOs.Staffs;
using BMOS.BAL.Helpers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Staffs
{
    public class PutStaffRequestValidation: AbstractValidator<UpdateStaffRequest>
    {
        private const int MAX_BYTES = 2048000;
        public PutStaffRequestValidation()
        {
            #region FullName
            RuleFor(s => s.FullName)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .Length(5, 40).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Address
            RuleFor(s => s.Address)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .Length(5, 90).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Phone
            RuleFor(s => s.Phone)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .Matches("^[0-9]*$").WithMessage("{PropertyName} contain only number.")
                .Length(10).WithMessage("{PropertyName} contains exactly 10 characters.");
            #endregion

            #region Avatar
            RuleFor(s => s.Avatar)
                .Custom((avatar, context) =>
                {
                    if (avatar != null)
                    {
                        if (avatar.Length < 1 || avatar.Length > MAX_BYTES)
                        {
                            context.AddFailure($"Image is required file length greater than 0 and less than {MAX_BYTES / 1024 / 1024} MB.");
                        }
                        if (FileHelper.HaveSupportedFileType(avatar.FileName) == false)
                        {
                            context.AddFailure("Avatar is required extension type .png, .jpg, .jpeg");
                        }
                    }
                });
            #endregion

            #region Gender
            RuleFor(s => s.Gender)
                .Must(x => x == true || x == false).WithMessage("{PropertyName} must be either true or false.");
            #endregion


            #region Birth Date
            RuleFor(s => s.BirthDate)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.");
            #endregion

            #region Password
            RuleFor(a => a.PasswordHash)
              .NotEmpty().WithMessage("{PropertyName} is empty.")
              .NotNull().WithMessage("{PropertyName} is null.")
              .Length(8, 20).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Status
            RuleFor(s => s.Status)
                .Must(x => x == true || x == false).WithMessage("{PropertyName} must be either true or false.");
            #endregion
        }
    }
}
