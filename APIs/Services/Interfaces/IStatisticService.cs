using System;
using BusinessObjects.DTO;
using BusinessObjects.Models.Utils;

namespace APIs.Services.Interfaces
{
	public interface IStatisticService
	{
		Task AddNewStats(Statistic stats);
		void UpdateStats(Statistic stats);
		Task<IEnumerable<Statistic>> GetAllAsync();
        Task<Statistic?> GetStatById(Guid id);
		void DeleteStats(Guid statId);
		Statistic CheckNewData(Statistic oldData, UpdateStatDTO dto);

    }
}

