using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Helpers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Validators.Products
{
    public class PostProductValidation : AbstractValidator<PostProductRequest>
    {

        private const int MAX_BYTES = 2048000;
        public PostProductValidation()
        {
            #region Name
            RuleFor(p => p.Name)
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .Length(5, 120).WithMessage("{PropertyName} from {MinLength} to {MaxLength} characters.");
            #endregion

            #region Description
            RuleFor(p => p.Description)
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .Length(5, 1000).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.");
            #endregion

            #region Total
            RuleFor(p => p.Total)
                 .NotNull().WithMessage("{PropertyName} is null.")
                 .NotEmpty().WithMessage("{PropertyName} is empty.")
                 .ExclusiveBetween(0, 101).WithMessage("{PropertyName} must be between 1 kilogram and 100 kilogram.");
            #endregion


            #region ExpiredDate
            RuleFor(p => p.ExpiredDate)
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .Custom((expiredDate, context) =>
                   {
                       if(expiredDate.Date.CompareTo(DateTime.Now.Date) < 0)
                       {
                           context.AddFailure("Expired Date must be greater than or equal today.");
                       }
                   });
            #endregion

            #region ProductImages
            RuleFor(p => p.ProductImages)
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .NotEmpty().WithMessage("{PropertyName} is empty.");
            RuleForEach(p => p.ProductImages)
                   .ChildRules(pro => pro.RuleFor(img => img.Length).ExclusiveBetween(0, MAX_BYTES)
                   .WithMessage($"Image is required file length greater than 0 and less than {MAX_BYTES / 1024 / 1024} MB"));
            RuleForEach(p => p.ProductImages)
                   .ChildRules(pro => pro.RuleFor(img => img.FileName).Must(FileHelper.HaveSupportedFileType).WithMessage("Product Image is required extension type .png, .jpg, .jpeg"));
            #endregion

            #region Price
            RuleFor(p => p.Price)
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .NotEmpty().WithMessage("{PropertyName} is empty.")
                   .ExclusiveBetween(0, 1000000000).WithMessage("{PropertyName} must be between 1 VND and 999.999.999 VND.");
            #endregion
            
            #region Original Price
            RuleFor(p => p.OriginalPrice)
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .NotEmpty().WithMessage("{PropertyName} is empty.")
                   .ExclusiveBetween(0, 1000000000).WithMessage("{PropertyName} must be between 1 VND and 999.999.999 VND.");
            #endregion
        }


    }
}
