using AWQP.Application.Common;
using AWQP.Application.DTOs;

namespace AWQP.Application.Interfaces;

public interface IWorkPermitService
{
    Task<ProgressWorkPermitFrontPageDto> GetProgressFrontPageAsync(string supplierCode, CancellationToken cancellationToken = default);
    Task<Result<ProgressWorkPermitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProgressWorkPermitDto>> UpdateProgressAsync(Guid id, UpdateProgressWorkPermitRequest request, CancellationToken cancellationToken = default);
    Task<Result<DailyAtmosphericReadingDto>> AddDailyReadingAsync(Guid workPermitId, AddDailyAtmosphericReadingRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AtmosphericParameterRangeDto>> GetAcceptableRangesAsync(CancellationToken cancellationToken = default);
}
