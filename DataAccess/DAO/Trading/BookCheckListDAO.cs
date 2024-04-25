using BusinessObjects;
using BusinessObjects.Models.Trading;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Trading
{
	public class BookCheckListDAO: GenericDAO<BookCheckList>
    {
		public BookCheckListDAO(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookCheckList>> GetCheckListByTradeDetailsId(Guid id)
        {
            var query = from cl in _context.BookCheckLists
                        join td in _context.TradeDetails on cl.TradeDetailsId equals td.TradeDetailId
                        where td.TradeDetailId == id
                        select cl;
            return await query.ToListAsync();

        }

        public async Task AddMultipleCheckList(List<BookCheckList> checkLists)
        {
            foreach(var cl in checkLists)
            {
                await _context.BookCheckLists.AddAsync(cl);
            }
        }

        public async Task<bool> IsCheckListExisted(Guid tradeDetailsId)
        => await _context.BookCheckLists.AnyAsync(c => c.TradeDetailsId == tradeDetailsId);

        public async Task<bool> IsAnyInChecklistNotSubmitted(Guid tradeDetailsId, string type)
        {
            IQueryable<BookCheckList> query = _context.TradeDetails
                .Join(_context.BookCheckLists, td => td.TradeDetailId, cl => cl.TradeDetailsId, (td, cl) => cl)
                .Where(cl => cl.TradeDetailsId == tradeDetailsId);

            if (type == "Trader")
            {
                query = query.Where(cl => cl.BookOwnerUploadDir == null);
            }
            else
            {
                query = query.Where(cl => cl.MiddleUploadDir == null);
            }

            return await query.AnyAsync();
        }

        public override void Update(BookCheckList data)
        {
            var existingPost = _dbSet.Find(data.Id);

            if (existingPost == null)
            {
                return;
            }

            _dbSet.Entry(existingPost).CurrentValues.SetValues(data);

            foreach (var property in _dbSet.Entry(existingPost).Properties)
            {
                if (property.IsModified && property.CurrentValue == null)
                {
                    property.IsModified = false;
                }
            }
        }
    }
}


