using System;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using DataAccess.DAO;

namespace APIs.Services
{
    public class AddressService : IAddressService
    {
        public int AddNewAddress(Address address) => new AddressDAO().AddNewAddress(address);

        public async Task<int> DeleteAddressAsync(Guid addressId) => (await new AddressDAO().DeleteAddressAsync(addressId));

        public PagedList<Address> GetAllUserAddress(Guid userId, PagingParams @params)
        {
            return PagedList<Address>.ToPagedList(new AddressDAO().GetAllUserAddress(userId).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        public async Task<Guid?> CheckAddressExisted(CompareAddressDTO dto)
        => await new AddressDAO().CheckAddressExisted(dto);

        public int UpdateAddressDefault(Address address) => new AddressDAO().UpdateAddressDefault(address);

        public async Task<Address?> GetAddressByIdAsync(Guid addressId)
        => await new AddressDAO().GetAddressByIdAsync(addressId);
    }
}

