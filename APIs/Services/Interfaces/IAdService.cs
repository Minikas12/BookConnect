using BusinessObjects.Models;
using BusinessObjects.Models.Ads;

namespace APIs.Services.Interfaces
{
	public interface IAdService
	{
        Task AddNewAd(Advertisement ad);
        void UpdateAd(Advertisement ad);
        Task<IEnumerable<Advertisement>> GetAllAsync();
        Task<Advertisement?> GetAdById(Guid id);
        void DeleteAd(Guid adId);
        IEnumerable<Book> GetRecommendedRelevantBooks(Guid bookId);
        Task<bool> IsTransactionBennAssigned(Guid txnId);

    }
}

