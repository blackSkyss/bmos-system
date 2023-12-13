using BMOS.DAL.Enums;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DataSeedings
{
    public static class MealDatatSeeding
    {
        public static void MealData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meal>().HasData(
                new Meal()
                {
                    ID = 1,
                    Title = "Khẩu Phần Ăn 1",
                    Description = "Mô tả khẩu phần ăn 1",
                    Status = (int)MealEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                },

                new Meal()
                {
                    ID = 2,
                    Title = "Khẩu Phần Ăn 2",
                    Description = "Mô tả khẩu phần ăn 2",
                    Status = (int)MealEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                },

                new Meal()
                {
                    ID = 3,
                    Title = "Khẩu Phần Ăn 3",
                    Description = "Mô tả khẩu phần ăn 3",
                    Status = (int)MealEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                }
            );
        }
    }
}
