using BMOS.DAL.DAOs;
using BMOS.DAL.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Infrastructures
{
    public class UnitOfWork : IUnitOfWork
    {
        private BMOSDbContext _dbContext;
        private AccountDAO _accountDAO;
        private CustomerDAO _customerDAO;
        private MealDAO _mealDAO;
        private MealImageDAO _mealImageDAO;
        private OrderDAO _orderDAO;
        private OrderDetailDAO _orderDetailDAO;
        private ProductDAO _productDAO;
        private ProductImageDAO _productImageDAO;
        private ProductMealDAO _productMealDAO;
        private RoleDAO _roleDAO;
        private StaffDAO _staffDAO;
        private TokenDAO _tokenDAO;
        private WalletDAO _walletDAO;
        private WalletTransactionDAO _walletTransactionDAO;
        private OrderTransactionDAO _orderTransactionDAO;
        private DashBoardDAO _dashBoardDAO;
        public UnitOfWork(IDbFactory dbFactory)
        {
            if (this._dbContext == null)
            {
                this._dbContext = dbFactory.InitDbContext();
            }
        }

        public AccountDAO AccountDAO
        {
            get
            {
                if (_accountDAO == null)
                {
                    _accountDAO = new AccountDAO(_dbContext);
                }
                return _accountDAO;
            }
        }

        public CustomerDAO CustomerDAO
        {
            get
            {
                if (_customerDAO == null)
                {
                    _customerDAO = new CustomerDAO(_dbContext);
                }
                return _customerDAO;
            }
        }

        public MealDAO MealDAO
        {
            get
            {
                if (_mealDAO == null)
                {
                    _mealDAO = new MealDAO(_dbContext);
                }
                return _mealDAO;
            }
        }

        public MealImageDAO MealImageDAO
        {
            get
            {
                if (_mealImageDAO == null)
                {
                    _mealImageDAO = new MealImageDAO(_dbContext);
                }
                return _mealImageDAO;
            }
        }

        public WalletDAO WalletDAO
        {
            get
            {
                if (_walletDAO == null)
                {
                    _walletDAO = new WalletDAO(_dbContext);
                }
                return _walletDAO;
            }
        }

        public WalletTransactionDAO WalletTransactionDAO
        {
            get
            {
                if (_walletTransactionDAO == null)
                {
                    _walletTransactionDAO = new WalletTransactionDAO(_dbContext);
                }
                return _walletTransactionDAO;
            }
        }

        public OrderDAO OrderDAO
        {
            get
            {
                if (_orderDAO == null)
                {
                    _orderDAO = new OrderDAO(_dbContext);
                }
                return _orderDAO;
            }
        }

        public OrderDetailDAO OrderDetailDAO
        {
            get
            {
                if (_orderDetailDAO == null)
                {
                    _orderDetailDAO = new OrderDetailDAO(_dbContext);
                }
                return _orderDetailDAO;
            }
        }

        public OrderTransactionDAO OrderTransactionDAO
        {
            get
            {
                if (_orderTransactionDAO == null)
                {
                    _orderTransactionDAO = new OrderTransactionDAO(_dbContext);
                }
                return _orderTransactionDAO;
            }
        }

        public ProductDAO ProductDAO
        {
            get
            {
                if (_productDAO == null)
                {
                    _productDAO = new ProductDAO(_dbContext);
                }
                return _productDAO;
            }
        }

        public ProductImageDAO ProductImageDAO
        {
            get
            {
                if (_productImageDAO == null)
                {
                    _productImageDAO = new ProductImageDAO(_dbContext);
                }
                return _productImageDAO;
            }
        }

        public ProductMealDAO ProductMealDAO
        {
            get
            {
                if (_productMealDAO == null)
                {
                    _productMealDAO = new ProductMealDAO(_dbContext);
                }
                return _productMealDAO;
            }
        }

        public RoleDAO RoleDAO
        {
            get
            {
                if (_roleDAO == null)
                {
                    _roleDAO = new RoleDAO(_dbContext);
                }
                return _roleDAO;
            }
        }

        public StaffDAO StaffDAO
        {
            get
            {
                if (_staffDAO == null)
                {
                    _staffDAO = new StaffDAO(_dbContext);
                }
                return _staffDAO;
            }
        }

        public TokenDAO TokenDAO
        {
            get
            {
                if (_tokenDAO == null)
                {
                    _tokenDAO = new TokenDAO(_dbContext);
                }
                return _tokenDAO;
            }
        }

        public DashBoardDAO DashBoardDAO
        {
            get
            {
                if (this._dashBoardDAO == null)
                {
                    this._dashBoardDAO = new DashBoardDAO(_dbContext);
                }
                return this._dashBoardDAO;
            }
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
