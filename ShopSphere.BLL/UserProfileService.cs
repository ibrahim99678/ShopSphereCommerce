using Microsoft.AspNetCore.Identity;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Identity;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class UserProfileService : IUserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return new UserProfileDto
        {
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    public async Task UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.FullName = profile.FullName;
        user.PhoneNumber = profile.PhoneNumber;
        user.ProfileImageUrl = profile.ProfileImageUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(message);
        }
    }

    public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var addresses = await _unitOfWork.Addresses.GetAllAsync(cancellationToken);
        return addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAtUtc)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<AddressDto> CreateAddressAsync(string userId, AddressDto address, CancellationToken cancellationToken = default)
    {
        var entity = new Address
        {
            UserId = userId,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            Country = address.Country,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault
        };

        await _unitOfWork.Addresses.AddAsync(entity, cancellationToken);

        if (entity.IsDefault)
        {
            await ClearDefaultAsync(userId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        address.Id = entity.Id;
        return address;
    }

    public async Task<bool> UpdateAddressAsync(string userId, AddressDto address, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Addresses.GetByIdAsync(address.Id, cancellationToken);
        if (entity is null || entity.UserId != userId)
        {
            return false;
        }

        entity.FullName = address.FullName;
        entity.PhoneNumber = address.PhoneNumber;
        entity.AddressLine1 = address.AddressLine1;
        entity.AddressLine2 = address.AddressLine2;
        entity.City = address.City;
        entity.Country = address.Country;
        entity.PostalCode = address.PostalCode;

        if (address.IsDefault && !entity.IsDefault)
        {
            await ClearDefaultAsync(userId, cancellationToken);
            entity.IsDefault = true;
        }
        else if (!address.IsDefault && entity.IsDefault)
        {
            entity.IsDefault = false;
        }

        _unitOfWork.Addresses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAddressAsync(string userId, int addressId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Addresses.GetByIdAsync(addressId, cancellationToken);
        if (entity is null || entity.UserId != userId)
        {
            return false;
        }

        _unitOfWork.Addresses.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetDefaultAddressAsync(string userId, int addressId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Addresses.GetByIdAsync(addressId, cancellationToken);
        if (entity is null || entity.UserId != userId)
        {
            return false;
        }

        await ClearDefaultAsync(userId, cancellationToken);
        entity.IsDefault = true;
        _unitOfWork.Addresses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ClearDefaultAsync(string userId, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Addresses.GetAllAsync(cancellationToken);
        foreach (var address in all.Where(a => a.UserId == userId && a.IsDefault))
        {
            address.IsDefault = false;
            _unitOfWork.Addresses.Update(address);
        }
    }

    private static AddressDto MapToDto(Address entity)
    {
        return new AddressDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            PhoneNumber = entity.PhoneNumber,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            IsDefault = entity.IsDefault
        };
    }
}

