using BMOS.DAL.DBContext;
using BMOS.DAL.Enums;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class CustomerDAO
    {
        private BMOSDbContext _dbContext;
        public CustomerDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Customer> GetCustomerByAccountIdAsync(int accountId)
        {
            try
            {
                return await _dbContext.Customers.Include(c => c.Account).ThenInclude(x => x.Role)
                                                 .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od => od.Meal).ThenInclude(m => m.MealImages)
                                                 .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od => od.Meal).ThenInclude(m => m.ProductMeals).ThenInclude(pm => pm.Product).ThenInclude(p => p.ProductImages)
                                                 .Include(c => c.Orders).ThenInclude(o => o.OrderTransactions)
                                                 .SingleOrDefaultAsync(c => c.AccountID == accountId && c.Account.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateCustomerProfile(Customer customer)
        {
            try
            {
                this._dbContext.Entry<Customer>(customer).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Get customer by email
        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            try
            {
                return await _dbContext.Customers.Include(c => c.Account)
                                                 .SingleOrDefaultAsync(c => c.Account.Email.Equals(email) 
                                                 && c.Account.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get customer by phone
        public async Task<Customer> GetCustomerByPhoneAsync(string phone)
        {
            try
            {
                return await _dbContext.Customers.Include(c => c.Account)
                                                 .SingleOrDefaultAsync(c => c.Phone.Equals(phone));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get customer by id
        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Customers.Include(c => c.Account)
                                                 .Include(c => c.Orders).ThenInclude(x => x.OrderDetails).ThenInclude(x => x.Meal).ThenInclude(x => x.ProductMeals).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
                                                 .Include(c => c.Orders).ThenInclude(x => x.OrderTransactions)
                                                 .SingleOrDefaultAsync(c => c.AccountID == id 
                                                 && c.Account.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        //get all customers
        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                return await _dbContext.Customers.Include(c => c.Account).ThenInclude(x => x.Role)
                                                 .Include(c => c.Orders)
                                                 .ThenInclude(o => o.OrderDetails)
                                                 .ThenInclude(od => od.Meal)
                                                 .Include(c => c.Orders)
                                                 .ThenInclude(o => o.OrderTransactions)
                                                 .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //ban customer by updating status
        public void BanCustomer(Customer customer)
        {
            try
            {
                this._dbContext.Entry<Customer>(customer).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddNewCustomer(Customer customer)
        {
            try
            {
              await _dbContext.Customers.AddAsync(customer);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
