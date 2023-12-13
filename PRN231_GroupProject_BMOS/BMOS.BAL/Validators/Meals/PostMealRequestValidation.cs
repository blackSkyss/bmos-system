using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.Helpers;
using FluentValidation;

namespace BMOS.BAL.Validators.Meals
{
    public class PostMealRequestValidation : AbstractValidator<PostMealRequest>
    {
        private const int MAX_BYTES = 2048000;
        public PostMealRequestValidation()
        {
            RuleFor(meal => meal.Title)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .MaximumLength(120).WithMessage("{PropertyName} max length is 120 characters.");

            RuleFor(RuleFor => RuleFor.Description)
                .NotNull().WithMessage("{PropertyName} is null.")
                .NotEmpty().WithMessage("{PropertyName} is empty.")
                .MaximumLength(1000).WithMessage("{PropertyName} max length is 1000 characters.");
            RuleFor(p => p.MealFileImages)
                   .NotNull().WithMessage("{PropertyName} is null.")
                   .NotEmpty().WithMessage("{PropertyName} is empty.");
            RuleForEach(p => p.MealFileImages)
                   .ChildRules(pro => pro.RuleFor(img => img.Length).ExclusiveBetween(0, MAX_BYTES)
                   .WithMessage($"Image is required file length greater than 0 and less than {MAX_BYTES / 1024 / 1024} MB"));
            RuleForEach(p => p.MealFileImages)
                   .ChildRules(pro => pro.RuleFor(img => img.FileName).Must(FileHelper.HaveSupportedFileType).WithMessage("Meal Image is required extension type .png, .jpg, .jpeg"));

            RuleFor(m => m.ProductMeals).NotNull().WithMessage("{PropertyName} is null.")
                                        .NotEmpty().WithMessage("{PropertyName} is empty.");

            RuleForEach(p => p.ProductMeals)
                .ChildRules(pm => pm.RuleFor(p => p.ProductID).NotNull().WithMessage("[Product Id - {PropertyValue}] is null.")
                                                              .NotEmpty().WithMessage("[Product Id - {PropertyValue}] is empty."));

            RuleForEach(p => p.ProductMeals)
                .ChildRules(pm => pm.RuleFor(p => p.Amount).ExclusiveBetween(0, 101).WithMessage("Amount is required more than 0 and less than or equal 100"));

        }
    }
}
