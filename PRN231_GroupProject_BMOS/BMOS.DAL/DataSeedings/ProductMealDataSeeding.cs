using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DataSeedings
{
    public static class ProductMealDataSeeding
    {
        public static void ProducMealtData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductMeal>().HasData(
                new ProductMeal()
                {
                    MealID = 1,
                    ProductID = 1,
                    Amount = 5,
                    Price = 100000,
                },

                new ProductMeal()
                {
                    MealID = 1,
                    ProductID = 2,
                    Amount = 10,
                    Price = 150000,
                },

                new ProductMeal()
                {
                    MealID = 1,
                    ProductID = 3,
                    Amount = 10,
                    Price = 100000,
                },

                new ProductMeal()
                {
                    MealID = 2,
                    ProductID = 1,
                    Amount = 4,
                    Price = 75000,
                },

                new ProductMeal()
                {
                    MealID = 2,
                    ProductID = 3,
                    Amount = 5,
                    Price = 100000,
                }
            );
        }
    }
}
