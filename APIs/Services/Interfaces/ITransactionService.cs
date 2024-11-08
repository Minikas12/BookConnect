﻿using System;
using APIs.Utils.Paging;
using BusinessObjects.Models.Ecom.Payment;

namespace APIs.Services.Interfaces
{
	public interface ITransactionService
	{
        public int AddTransactionRecord(TransactionRecord transaction);
        public TransactionRecord? GetTransactionById(Guid refId);
        int IdentifyTransactor(Guid transId, Guid userId);
        IQueryable<TransactionRecord> GetAllTransaction();
    }
}

