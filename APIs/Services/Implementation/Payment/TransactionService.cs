
using APIs.Services.Interfaces;
using BusinessObjects.Models.Ecom.Payment;
using DataAccess.DAO;

namespace APIs.Services.Payment
{
    public class TransactionService : ITransactionService
    {
        public int AddTransactionRecord(TransactionRecord transaction) => new TransactionDAO().AddTransactionRecord(transaction);

        public IQueryable<TransactionRecord> GetAllTransaction() => new TransactionDAO().GetAllTransaction();
      
        public TransactionRecord? GetTransactionById(Guid refId) => new TransactionDAO().GetTransactionById(refId);
        public int IdentifyTransactor(Guid transId, Guid userId) => new TransactionDAO().IdentifyTransactor(transId, userId);
    }
}

