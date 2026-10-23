using CYHDocBrandData.Entities;
using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IDoctorRepository
{
    // Auth via Stored Procedure
    Task<CurrentUserDTO?> AuthenticateAsync(string email, string password);

    Task<List<DoctorProfile>> GetAllWithUsersAsync();
    Task<DoctorProfile?> GetByIdAsync(int doctorId);
    Task<DoctorProfile?> GetByUserIdAsync(string userId);
    Task<bool> ToggleAvailabilityAsync(int doctorId);
    Task<bool> UpdateProfileAsync(DoctorProfile profile);
}
