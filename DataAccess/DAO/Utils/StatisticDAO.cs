using BusinessObjects;
using BusinessObjects.Models.Utils;

namespace DataAccess.DAO.Utils
{
	public class StatisticDAO : GenericDAO<Statistic>
	{
		public StatisticDAO(AppDbContext context): base(context)
		{
			_context = context;
		}

        public void UpdateAsync(Statistic stats)
        {
                    var existingPost = _dbSet.Find(stats.StatId);

                    if (existingPost == null)
                    {
                        return;
                    }

                   _dbSet.Entry(existingPost).CurrentValues.SetValues(stats);

                    foreach (var property in _dbSet.Entry(existingPost).Properties)
                    {
                        if (property.IsModified && property.CurrentValue == null)
                        {
                            property.IsModified = false;
                        }
                    }
                }
        public Dictionary<Guid, int> ViewOfCateByTime(DateTime startDate, DateTime endDate)
        {
            var query = (from st in _context.Statistics
                        join p in _context.Posts on st.PostId equals p.PostId
                        join cl in _context.CategoryLists on p.PostId equals cl.PostId
                        where st.LastUpdate <= endDate && st.LastUpdate >= startDate
                        select new { cl.CategoryId, st.View }).ToList();
            Dictionary<Guid, int> result = new Dictionary<Guid, int>();
            foreach(var q in query)
            {
                result.Add(q.CategoryId, q.View);
            }
            return result;
        }
    }
       
 }

