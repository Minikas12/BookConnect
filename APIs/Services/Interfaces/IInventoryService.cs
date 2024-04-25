using BusinessObjects.Models;
using BusinessObjects.Models.Utils;
using CloudinaryDotNet.Actions;
using DataAccess.DAO;
using APIs.Utils.Paging;
namespace APIs.Services.Interfaces
{
    public interface IInventoryService
    {
        PagedList<Inventory> GetAllInventory(PagingParams param);
        public Inventory GetInventoryById(Guid AgencyId,Guid ProductId);
        public int UpdateInventory(Inventory invent);
        public int AddNewInventory(Inventory invent);
    }
}
