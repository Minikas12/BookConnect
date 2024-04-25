using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;

namespace APIs.Services.Interfaces
{
	public interface IAddressService
	{
		int AddNewAddress(Address address);
		PagedList<Address> GetAllUserAddress(Guid userId, PagingParams @params);
		int UpdateAddressDefault(Address address);
		Task<int> DeleteAddressAsync(Guid addressId);
        Task<Guid?> CheckAddressExisted(CompareAddressDTO dto);
        Task<Address?> GetAddressByIdAsync(Guid addressId);
    }
}

