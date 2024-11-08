﻿using System;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
	public class AddressDAO
	{
		private readonly AppDbContext _context;
		public AddressDAO()
        {
            _context = new AppDbContext();
        }
        //Get all address of a user
        public List<Address> GetAllUserAddress(Guid userId)
		{
			List<Address> result = new List<Address>();
			try
			{
				using (var context = new AppDbContext())
				{
					result = context.Addresses.Where(u => u.UserId == userId).ToList();
                    return result;
                }

            }
            catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public async Task<Address?> GetAddressByIdAsync(Guid addressId)
		=> await _context.Addresses.SingleOrDefaultAsync(a => a.AddressId == addressId);

		public Address? GetUserDefaultAddress(Guid userId)
		{
			try
			{
				using(var context = new AppDbContext())
				{
					//Handle this null warning
					return context.Addresses.Where(a => a.UserId == userId && a.Default == true).FirstOrDefault();
				}
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public int AddNewAddress(Address address)
		{
			try
			{
				using(var context = new AppDbContext())
				{
					context.Addresses.Add(address);
					if (address.Default)
					{
						SetAddressDefault(address.AddressId);
					}
					return context.SaveChanges();
				}
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public int SetAddressDefault(Guid addressId)
		{
			try
			{
				using(var context = new AppDbContext())
				{
					Address? address = context.Addresses.Where(a => a.AddressId == addressId).SingleOrDefault();
					if(address != null)
					{
                        List<Address> records = context.Addresses.Where(a => a.UserId == address.UserId && a.Default && a.AddressId != addressId).ToList();
                        foreach (Address a in records)
                        {
                            a.Default = false;
                        }
                        address.Default = true;
                    }
                    return context.SaveChanges();
				}
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public int UpdateAddressDefault(Address address)
		{
			try
			{
				using(var context = new AppDbContext())
				{
					int result = 0;
					if(address.UserId != null)
					{
						if (IsOldAddress(address.AddressId, (Guid)address.UserId))
						{
							result += SetAddressDefault(address.AddressId);
						}
						else context.Addresses.Add(address);
						result += context.SaveChanges();
						SetAddressDefault(address.AddressId);
						result += context.SaveChanges();
                    }
                    result += context.SaveChanges();
                    return result;
                }
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public bool IsOldAddress(Guid addressId, Guid userId)
		{
			try
			{
				using(var context = new AppDbContext())
				{
					return context.Addresses.Any(a => a.AddressId == addressId && a.UserId == userId);
				}
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public List<Address> SearchOldAddress(string inputString, Guid userId)
		{
			try
			{
				using (var context = new AppDbContext())
				{
					
					List<Address> result = new List<Address>();
					var matchedCates = context.Addresses
					.Where(c => c.Rendezvous.Contains(inputString))
					.ToList();
					if (matchedCates.Count > 0)
					{
						result = matchedCates;
					}
					return result;
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public async Task<int> DeleteAddressAsync(Guid addressId)
		{
			Address? address = await _context.Addresses.SingleOrDefaultAsync(a => a.AddressId == addressId);
			if(address != null)
			{
				_context.Addresses.Remove(address);
			}
			return await _context.SaveChangesAsync();
		}

		public async Task<Guid?> CheckAddressExisted(CompareAddressDTO dto)
		{
			Address? record = await _context.Addresses.SingleOrDefaultAsync(a => a.UserId == dto.UserId && a.City_Province == dto.City_Province && a.District == dto.District
			&& a.SubDistrict == dto.SubDistrict && a.Rendezvous == dto.Rendezvous);
			if(record != null)
			{
				return record.AddressId;
			} return null;
		}
	}
}

