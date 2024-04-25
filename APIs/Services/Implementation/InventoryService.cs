using APIs.Utils.Paging;
using BusinessObjects.Models.Utils;
using DataAccess.DAO;
using CloudinaryDotNet.Actions;
using BusinessObjects.Models;
using APIs.Services.Interfaces;

namespace APIs.Services
{
    public class InventoryService : IInventoryService
    {
        public PagedList<Inventory> GetAllInventory(PagingParams param)
        {
            return PagedList<Inventory>.ToPagedList(new InventoryDAO().GetAllInventory().OrderBy(bg => bg.InventoryId).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public Inventory GetInventoryById(Guid AgencyId,Guid ProductId) => new InventoryDAO().GetInventoryById(AgencyId,ProductId);
        public int AddNewInventory(Inventory invent) => new InventoryDAO().AddNewInventory(invent);
        public int UpdateInventory(Inventory invent) => new InventoryDAO().UpdateInventory(invent);
    }
}