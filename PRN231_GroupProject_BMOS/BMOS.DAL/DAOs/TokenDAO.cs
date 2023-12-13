using BMOS.DAL.DBContext;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class TokenDAO
    {
        private BMOSDbContext _dbContext;
        public TokenDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Get token by refresh token
        public async Task<Token> GetTokenByRefreshTokenAsync(string token)
        {
            try
            {
                return await _dbContext.Tokens.Include(t => t.Account)
                                              .ThenInclude(a => a.Role)
                                              .SingleOrDefaultAsync(t => t.RefreshToken.Equals(token));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        #endregion

        #region Create token
        public async Task CreateTokenAsync(Token token)
        {
            try
            {
                await this._dbContext.Tokens.AddAsync(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update Token
        public void UpdateToken(Token token)
        {
            try
            {
                this._dbContext.Entry<Token>(token).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
