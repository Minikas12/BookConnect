using System;
using APIs.Services.Interfaces;
using DataAccess.DAO;

namespace APIs.Services
{
	public class UserService: IUserService
	{
		private readonly AccountDAO _accountDAO;
		public UserService()
		{
			_accountDAO = new AccountDAO();
		}

	}
}

