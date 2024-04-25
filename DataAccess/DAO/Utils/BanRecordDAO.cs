using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.Ecom.Base;

namespace DataAccess.DAO
{
    public class BanRecordDAO
    {
        private readonly AppDbContext _context;
        public BanRecordDAO()
        {
            _context = new AppDbContext();
        }
        public int AddBanRecord(BanRecord data)
        {
            _context.BanRecords.Add(data);
            return _context.SaveChanges();
        }

        public int ForceUnban(Guid userId, string reason)
        {

            DateTime? latestUnbannedDate = _context.BanRecords
           .Where(r => r.TargetUserId == userId)
           .OrderByDescending(r => r.UnbannedDate)
           .Select(r => r.UnbannedDate)
           .FirstOrDefault();
            BanRecord? record = _context.BanRecords.Where(r => r.TargetUserId == userId && r.UnbannedDate == latestUnbannedDate).SingleOrDefault();
            if (record != null)
            {
                record.UnbannedDate = DateTime.Now;
                record.UnBanReason = reason;
            }
            AppUser? user = _context.AppUsers.Where(u => u.UserId == userId).SingleOrDefault();
            if (user != null)
            {
                user.IsBanned = false;
            }
            return _context.SaveChanges();
        }
    }
}

