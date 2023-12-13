using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DataSeedings
{
    public static class RoleDataSeeding
    {
        public static void RoleData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role() { ID = 1, Name = "Store Owner" },
                new Role() { ID= 2, Name = "Staff"},
                new Role() { ID = 3, Name = "Customer"}
            );
        }
    }
}
