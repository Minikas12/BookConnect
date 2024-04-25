using BusinessObjects.Models.Trading;

namespace APIs.Services.Interfaces
{
    public interface IBookCheckListService
    {

        Task AddBookCheckList(BookCheckList model);

        Task<BookCheckList?> GetById(Guid id);

        void DeleteCheckList(Guid id);

        void UpdateCheckList(BookCheckList data);

        Task<IEnumerable<BookCheckList>> GetCheckListByTradeDetailsId(Guid id);

        Task AddMultipleCheckList(List<BookCheckList> checkLists);

        Task<bool> IsCheckListExisted(Guid tradeDetailsId);

        Task<bool> IsAnyInChecklistNotSubmitted(Guid tradeDetailsId, string type);
    }
}

