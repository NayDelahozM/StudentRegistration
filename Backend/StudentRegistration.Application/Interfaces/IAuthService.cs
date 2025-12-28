using StudentRegistration.Application.DTOs.Auth;
using StudentRegistration.Domain.Common;

namespace StudentRegistration.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
    }
}
