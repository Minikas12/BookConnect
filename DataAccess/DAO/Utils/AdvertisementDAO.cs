using BusinessObjects;
using BusinessObjects.Models.Ads;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Utils
{
	public class AdvertisementDAO: GenericDAO<Advertisement>
	{
		public AdvertisementDAO(AppDbContext context): base(context)
		{
			_context = context;
		}

		public async Task<bool> IsTransactionBennAssigned(Guid txnId)
		=> await _context.Advertisements.AnyAsync(a => a.TransactionId == txnId);
	}
}

