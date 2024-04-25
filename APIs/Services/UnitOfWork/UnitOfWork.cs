using APIs.Services.Implementation;
using APIs.Services.Interfaces;
using BusinessObjects;
using Microsoft.EntityFrameworkCore.Storage;

namespace APIs.Services
{
	public class UnitOfWork: IUnitOfWork
	{

        //put services here
        private readonly AppDbContext _context;
        private IStatisticService? _statisticService;
        private IAdService? _adsService;
        private IBookCheckListService? _bookCheckListService; 

        private bool _disposed = false;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IStatisticService StatisticService => _statisticService ?? (_statisticService = new StatisticService(_context));
      
        public IAdService AdsService => _adsService ?? (_adsService = new AdService(_context));

        public IBookCheckListService BookCheckListService => _bookCheckListService ?? (_bookCheckListService = new BookCheckListService(_context));

        public async Task<int> Save() => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

