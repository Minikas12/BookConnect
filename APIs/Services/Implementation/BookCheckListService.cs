using APIs.Services.Interfaces;
using BusinessObjects;
using BusinessObjects.Models.Trading;
using DataAccess.DAO.Trading;

namespace APIs.Services.Implementation
{
	public class BookCheckListService: IBookCheckListService
	{
		private BookCheckListDAO _bookCheckListDAO;
		public BookCheckListService(AppDbContext context)
		{
			_bookCheckListDAO = new BookCheckListDAO(context);
		}

		public async Task AddBookCheckList(BookCheckList model)
		=> await _bookCheckListDAO.Insert(model);

        public void DeleteCheckList(Guid id)
        => _bookCheckListDAO.DeleteById(id);

        public void UpdateCheckList(BookCheckList data)
        => _bookCheckListDAO.Update(data);

		public async Task<IEnumerable<BookCheckList>> GetCheckListByTradeDetailsId(Guid id)
		=> await _bookCheckListDAO.GetCheckListByTradeDetailsId(id);

		public async Task AddMultipleCheckList(List<BookCheckList> checkLists)
		=> await _bookCheckListDAO.AddMultipleCheckList(checkLists);

		public async Task<BookCheckList?> GetById(Guid id)
		=> await _bookCheckListDAO.GetById(id);

		public async Task<bool> IsCheckListExisted(Guid tradeDetailsId)
	   => await _bookCheckListDAO.IsCheckListExisted(tradeDetailsId);

		public async Task<bool> IsAnyInChecklistNotSubmitted(Guid tradeDetailsId, string type)
		=> await _bookCheckListDAO.IsAnyInChecklistNotSubmitted(tradeDetailsId, type);
    }
}

