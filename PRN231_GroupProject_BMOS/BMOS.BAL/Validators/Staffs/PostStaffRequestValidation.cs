using BMOS.BAL.DTOs.Staffs;
using BMOS.BAL.Helpers;
using BMOS.BAL.Validators.Accounts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Staffs
{
    public class PostStaffRequestValidation : AbstractValidator<PostStaffRequest>
    {
        private const int MAX_BYTES = 2048000;
        public PostStaffRequestValidation()
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
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .Custom((avatar, context) =>
                {
                    if(avatar != null)
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

            #region Identity Number
            RuleFor(s => s.IdentityNumber)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .Matches("^[0-9]*$").WithMessage("{PropertyName} contain only number.")
                .Length(12).WithMessage("{PropertyName} contains exactly 12 characters.");
            #endregion

            #region Birth Date
            RuleFor(s => s.BirthDate)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.");
            #endregion

            #region Account 
            RuleFor(a => a.Account)
                .SetValidator(new PostAccountValidation());
            #endregion
        }
    }
}
