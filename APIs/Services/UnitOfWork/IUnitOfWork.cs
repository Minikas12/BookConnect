using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace APIs.Services.Interfaces
{
	public interface IUnitOfWork: IDisposable
	{
		IStatisticService StatisticService { get; }
        IAdService AdsService { get; }
        IBookCheckListService BookCheckListService { get; }

        Task<IDbContextTransaction> BeginTransactionAsync();

        Task<int> Save();
    }
}

