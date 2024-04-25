using BusinessObjects.Models;
using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models.Utils;
using BusinessObjects.DTO;

namespace DataAccess.DAO
{
    public class InventoryDAO
    {
        public List<Inventory> GetAllInventory()
        {
            try
            {
                List<Inventory> result = new List<Inventory>();
                using (var context = new AppDbContext())
                {
                    if (context.Inventories.Any())
                    {
                        result = context.Inventories.ToList();
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public Inventory GetInventoryById(Guid agencyId,Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return context.Inventories.FirstOrDefault(i => i.AgencyId == agencyId && i.ProductId==productId);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching inventory by agency id and product id", e);
            }
        }

        public int AddNewInventory(Inventory invent)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Inventories.Add(invent);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error adding new inventory", e);
            }
        }

        public int UpdateInventory(Inventory invent)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Inventories.Update(invent);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error updating inventory", e);
            }
        }


    }
}
