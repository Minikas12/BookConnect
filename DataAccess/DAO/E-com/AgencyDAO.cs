using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.DAO.Ecom
{
    public class AgencyDAO
    { 
        public NameAndIdDTO GetNameAndId(Guid bookId)
        {
            Guid agencyId = Guid.Empty;

            using (var context = new AppDbContext())
            {
                Inventory? invent = context.Inventories.Where(il => il.ProductId == bookId).FirstOrDefault();

                if (invent != null && invent.AgencyId != null)
                {
                    agencyId = invent.AgencyId;
                }

                NameAndIdDTO dto = new NameAndIdDTO()
                {
                    AgencyId = agencyId,
                    AgencyName = context.Agencies.Where(i => i.AgencyId == agencyId).FirstOrDefault()?.AgencyName,
                };
                return dto;
            }
        }
        public int GetProductQuantity(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Inventory? record = context.Inventories.Where(i => i.ProductId == productId).SingleOrDefault();

                    if (record == null) return 0;
                    else
                    {
                        return record.Quantity;
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int AddNewAgency(Agency agency)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Agencies.Add(agency);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> HasBookInInventory(Guid agencyId, Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Check if there exists an inventory entry for the specified agency and book
                    var inventoryEntry = await context.Inventories.FirstOrDefaultAsync(inv => inv.AgencyId == agencyId && inv.ProductId == bookId);

                    // Return true if an inventory entry is found, indicating the agency has the book in its inventory
                    return inventoryEntry != null;
                }
            }
            catch (Exception e)
            {
                // Log the error message along with the stack trace
                string errorMessage = $"An error occurred while checking inventory: {e.Message}. Stack Trace: {e.StackTrace}";
                Console.WriteLine(errorMessage);

                // Return false to indicate that an error occurred during the check
                return false;
            }
        }
        public Agency GetAgencyById(Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Agency? result = context.Agencies.Where(a => a.AgencyId == agencyId).SingleOrDefault();
                    if (result == null) { return new Agency(); }
                    else return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
       
        public List<Agency> GetAgencyByOwnerId(Guid ownerId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return context.Agencies.Where(a => a.OwnerId == ownerId).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int UpadateAgency(Agency updatedData)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Agencies.Update(updatedData);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Address? GetCurrentAddress(Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Address? address = (from ad in context.Addresses
                                        join ag in context.Agencies on ad.AddressId equals ag.PostAddressId
                                        select ad).SingleOrDefault();
                    return address;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string? GetCurrentLogoUrl(Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Agency? agency = context.Agencies.Where(a => a.AgencyId == agencyId).SingleOrDefault();
                    string? result = (agency != null) ? agency.LogoUrl : null;
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Agency> GetAllAgency()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return context.Agencies.ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task<Agency> GetAgencyByIdAsync(Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Agency? result = await context.Agencies.Where(a => a.AgencyId == agencyId).SingleOrDefaultAsync();
                    if (result == null) { return new Agency(); }
                    else return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while retrieving agency by ID {agencyId}: {e.Message}");
            }
        }

        public async Task<Guid> GetAgencyIdAsync(Guid ownerId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Query the database asynchronously to find the agency ID associated with the given owner ID
                    Agency agency = await context.Agencies.FirstOrDefaultAsync(a => a.OwnerId == ownerId);

                    // Return the agency ID if found, otherwise return Guid.Empty
                    return agency?.AgencyId ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                // Log the exception and handle it accordingly
                throw new Exception("Error occurred while fetching agency ID: " + e.Message);
            }
        }

        // ----------------------------------DATDQ---------------------------------------//

        public Guid GetAgencyId(Guid ownerId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Query the database to find the agency ID associated with the given owner ID
                    Agency agency = context.Agencies.FirstOrDefault(a => a.OwnerId == ownerId);

                    // Return the agency ID if found, otherwise return Guid.Empty
                    return agency?.AgencyId ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                // Log the exception and handle it accordingly
                throw new Exception("Error occurred while fetching agency ID: " + e.Message);
            }
        }

        public Guid GetInventoryId(Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Query the database to find the agency ID associated with the given agency ID
                    Inventory invent = context.Inventories.FirstOrDefault(i => i.AgencyId == agencyId);

                    // Return the inventory ID if found, otherwise return Guid.Empty
                    return invent?.InventoryId ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                // Log the exception and handle it accordingly
                throw new Exception("Error occurred while fetching agency ID: " + e.Message);
            }
        }

        public AgencyAnalystDTO GetAgencyAnalyst(Guid agencyId) //Update
        {
            AgencyAnalystDTO result = new AgencyAnalystDTO();
            OrderDAO o = new OrderDAO();
            BookDAO b = new BookDAO();
            try
            {
                (decimal thisMonthRevenue, decimal thisDayRevenue, decimal avgMonthRevenue,
            decimal avgDayRevenue, string highestMonth, string highestDay, decimal avgMonthPercentage,
            decimal avgDayPercentage, decimal highestMonthPercentage,
            decimal highestDayPercentage) stat = o.GetRevenueStats(agencyId);
                using (var context = new AppDbContext())
                    result = new AgencyAnalystDTO()
                    {
                        AgencyId = agencyId,
                        AgencyName = context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.AgencyName).FirstOrDefault(),
                        TotalQuantityOfBookInInventory = b.GetTotalQuantityByAgencyId(agencyId),
                        TotalBookSold = o.GetTotalNumberOfBookSold(agencyId),
                        TotalUnitSold = o.GetTotalNumberOfUnitSold(context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.OwnerId).FirstOrDefault()),
                        TotalRevenue = o.GetTotalRevenue(agencyId),
                        AvgMonthRevenue = stat.avgMonthRevenue,
                        AvgDayRevenue = stat.avgDayRevenue,
                        ThisMonthRevenue = stat.thisMonthRevenue,
                        ThisDayRevenue = stat.thisDayRevenue,
                        HighestMonthRevenue = stat.highestMonth,
                        HighestDayRevenue = stat.highestDay,
                        PercentThisMonthToAvgMonth = stat.avgMonthPercentage,
                        PercentThisDayToAvgDay = stat.avgDayPercentage,
                        PercentThisMonthToHighestMonth = stat.highestMonthPercentage,
                        PercentThisDayToHighestDay = stat.highestDayPercentage,
                        RevenueByMonths = o.GetRevenueByMonth(agencyId),
                        RevenueByDays = o.GetRevenueByDay(agencyId), //Update
                        RevenueByType = o.GetTotalRevenueByType(agencyId),
                        RevenueByCategory = o.GetTotalRevenueByCategory(agencyId),
                        NumberOfBookAndUnitSoldByMonths = o.GetNumberOfBookSoldAndUnitByMonth(context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.OwnerId).FirstOrDefault(), agencyId),
                        NumberOfBookAndUnitSoldByDays = o.GetNumberOfBookSoldAndUnitByDay(context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.OwnerId).FirstOrDefault(), agencyId) //Update
                    };

                return result;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public AgencyAnalystTimeInputDTO GetAgencyAnalystByTime(Guid agencyId, DateTime startDate, DateTime endDate) //Update
        {
            AgencyAnalystTimeInputDTO result = new AgencyAnalystTimeInputDTO();
            OrderDAO o = new OrderDAO();
            try
            {
                using (var context = new AppDbContext())
                    result = new AgencyAnalystTimeInputDTO()
                    {
                        AgencyId = agencyId,
                        AgencyName = context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.AgencyName).FirstOrDefault(),
                        Revenue = o.GetTotalRevenueByTime(agencyId, startDate, endDate),
                        RevenueByTimeInput = o.GetRevenueByDayInput(agencyId, startDate, endDate),
                        NumberOfBookAndUnitSoldByTimeInput = o.GetNumberOfBookSoldAndUnitByDayInput(context.Agencies.Where(a => a.AgencyId == agencyId).Select(a => a.OwnerId).FirstOrDefault(), agencyId, startDate, endDate)
                    };

                return result;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // ----------------------------------END DATDQ-----------------------------------//

        public Guid GetOwnerIdFromBookId(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var inventory = context.Inventories.FirstOrDefault(i => i.ProductId == bookId);
                    var agency = context.Agencies.FirstOrDefault((a => a.AgencyId == inventory.AgencyId));
                    if (inventory != null)
                    {
                        return agency.OwnerId;
                    }
                    else
                    {
                        throw new Exception("Owner ID not found for the book ID in inventory.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching owner ID from book ID.", ex);
            }
        }

    }

}

    
