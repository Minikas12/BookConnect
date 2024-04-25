using APIs.Services.Interfaces;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models.Utils;
using DataAccess.DAO.Utils;

namespace APIs.Services.Implementation
{
	public class StatisticService: IStatisticService
	{
        private readonly StatisticDAO _statDAO;
		public StatisticService(AppDbContext context)
		{
            _statDAO = new StatisticDAO(context);
		}

        public async Task AddNewStats(Statistic stats)
        => await _statDAO.Insert(stats);

        public void DeleteStats(Guid statId)
        => _statDAO.DeleteById(statId);

        public async Task<IEnumerable<Statistic>> GetAllAsync()
        => await _statDAO.GetAsync();

        public async Task<Statistic?> GetStatById(Guid id)
        => await _statDAO.GetById(id);

        public void UpdateStats(Statistic stats)
        => _statDAO.UpdateAsync(stats);

        public Statistic CheckNewData(Statistic oldData, UpdateStatDTO dto)
        {
            oldData.View += dto.View;
            oldData.Search += dto.Search;
            if(oldData.PostId == null)
            {
                oldData.Purchase += dto.Purchase;
            }
            else
            {
                oldData.Interested += dto.Interested;
                oldData.Hearts += dto.Hearts;
            }
            return oldData;
        }
    }
}

