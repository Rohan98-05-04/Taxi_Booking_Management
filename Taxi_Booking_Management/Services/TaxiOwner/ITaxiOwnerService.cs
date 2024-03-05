﻿using Taxi_Booking_Management.Models;
using X.PagedList;
namespace Taxi_Booking_Management.Services.TaxiOwner
{
    public interface ITaxiOwnerService
    {
        public Task<string> RegisterTaxiOwnerAsync(Models.TaxiOwner taxiOwner);
        public Task<Models.TaxiOwner?> GetTaxiOwnerAsync(int ownerId);
        public Task<IPagedList<Models.TaxiOwner>> GetAllTaxiOwnerAsync(int page, int pageSize, string search);
        public Task<int?> UpdateTaxiOwner(Models.TaxiOwner taxiOwner);
    }
}
