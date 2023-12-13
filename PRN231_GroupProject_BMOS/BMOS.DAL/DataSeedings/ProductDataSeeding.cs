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
    public static class ProductDataSeeding
    {
        public static void ProductData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product()
                {
                    ID = 1,
                    Name = "Cám hạt dẻ",
                    Description = "Mô tả của cám hạt dẻ",
                    Price = 10000,
                    ImportedTime = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMinutes(1),
                    Total = 10,
                    Status = (int)ProductEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                },

                new Product()
                {
                    ID = 2,
                    Name = "Sâu Khô",
                    Description = "Mô tả của sâu khô",
                    Price = 20000,
                    ImportedTime = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMinutes(1),
                    Total = 20,
                    Status = (int)ProductEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                },

                new Product()
                {
                    ID = 3,
                    Name = "Ngũ Cốc",
                    Description = "Mô tả của ngũ cốc",
                    Price = 50000,
                    ImportedTime = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddMinutes(1),
                    Total = 30,
                    Status = (int)ProductEnum.Status.STOCKING,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = 1
                }
            ); ;
        }
    }
}
