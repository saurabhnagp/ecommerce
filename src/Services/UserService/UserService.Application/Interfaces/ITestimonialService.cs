using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface ITestimonialService
{
    Task<IEnumerable<TestimonialDto>> GetActiveAsync(CancellationToken ct = default);
    Task<TestimonialDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TestimonialDto> CreateAsync(CreateTestimonialRequest request, CancellationToken ct = default);
    Task<TestimonialDto?> UpdateAsync(Guid id, UpdateTestimonialRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
