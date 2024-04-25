using APIs.Services.Interfaces;
using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.Ads;
using DataAccess.DAO.Utils;

namespace APIs.Services.Implementation
{
	public class AdService: IAdService
	{
		private readonly AppDbContext _context;
        private readonly AdvertisementDAO _advertisementDAO;
		public AdService(AppDbContext context)
		{
			_context = context;
            _advertisementDAO = new AdvertisementDAO(context);
		}

        public async Task AddNewAd(Advertisement ad)
        => await _advertisementDAO.Insert(ad);

        public void DeleteAd(Guid adId)
        => _advertisementDAO.DeleteById(adId);

        public async Task<Advertisement?> GetAdById(Guid id)
        => await _advertisementDAO.GetById(id);


        public async Task<IEnumerable<Advertisement>> GetAllAsync()
        => await _advertisementDAO.GetAsync();

        public IEnumerable<Book> GetRecommendedRelevantBooks(Guid bookId)
        {
			//sort here
			return _context.Books.ToList().OrderBy(b => b.CreatedDate).Take(18);
        }

        public Task<bool> IsTransactionBennAssigned(Guid txnId)
        => _advertisementDAO.IsTransactionBennAssigned(txnId);

        public void UpdateAd(Advertisement ad)
        => _advertisementDAO.Update(ad);
    }
}

