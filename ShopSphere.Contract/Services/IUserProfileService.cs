using ShopSphere.Contract.Dtos;

namespace ShopSphere.Contract.Services;

public interface IUserProfileService
{
    Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AddressDto>> GetAddressesAsync(string userId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAddressAsync(string userId, AddressDto address, CancellationToken cancellationToken = default);
    Task<bool> UpdateAddressAsync(string userId, AddressDto address, CancellationToken cancellationToken = default);
    Task<bool> DeleteAddressAsync(string userId, int addressId, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAddressAsync(string userId, int addressId, CancellationToken cancellationToken = default);
}

