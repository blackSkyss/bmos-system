using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BMOS.BAL.Scheduling
{
    //[DisallowConcurrentExecution]
    public class ProcessingWalletTransaction : IJob
    {
        private IWalletTransactionRepository _walletTransactionRepository;

        public ProcessingWalletTransaction(IWalletTransactionRepository walletTransactionRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _walletTransactionRepository.ProcessWalletTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
