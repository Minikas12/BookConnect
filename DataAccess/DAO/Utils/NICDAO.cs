using System;
using BusinessObjects;
using BusinessObjects.Models.Utils;

namespace DataAccess.DAO.Utils
{
	public class NICDAO
	{
		private readonly AppDbContext _context;
		public NICDAO(AppDbContext context)
		{
			_context = context;
		}

		public async Task<int> AddNicData(NIC_Data data)
		{
			await _context.NIC_Datas.AddAsync(data);
			return await _context.SaveChangesAsync();
		}

		
	}
}

