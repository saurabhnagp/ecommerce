using AmCart.UserService.Application.DTOs;

namespace AmCart.UserService.Application.Interfaces;

public interface IContactMessageService
{
    Task<ContactMessageDto> SubmitAsync(CreateContactMessageRequest request, CancellationToken ct = default);
    Task<IEnumerable<ContactMessageDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken ct = default);
}
