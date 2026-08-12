using AWQP.Application.Common;
using AWQP.Application.DTOs;

namespace AWQP.Application.Interfaces;

public interface ICheckSheetService
{
    // Check Point Master
    Task<IReadOnlyCollection<CheckPointMasterDto>> ListCheckPointsAsync(CancellationToken cancellationToken = default);
    Task<Result<CheckPointMasterDto>> GetCheckPointAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CheckPointMasterDto>> CreateCheckPointAsync(CreateCheckPointMasterRequest request, CancellationToken cancellationToken = default);
    Task<Result<CheckPointMasterDto>> UpdateCheckPointAsync(Guid id, UpdateCheckPointMasterRequest request, CancellationToken cancellationToken = default);
    Task<Result<CheckPointMasterDto>> UpdateCheckPointSpecsAsync(Guid id, UpdateCheckPointSpecsRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteCheckPointAsync(Guid id, CancellationToken cancellationToken = default);

    // Cascading lookups for Items Sorting
    Task<IReadOnlyCollection<CheckSheetLookupDto>> ListDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CheckSheetLookupDto>> ListSectionsAsync(string departmentCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> ListMachineNamesAsync(string departmentCode, string sectionCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> ListFrequenciesAsync(string departmentCode, string sectionCode, string machineName, CancellationToken cancellationToken = default);
    Task<Result<CheckSheetDefinitionDto>> GetDefinitionAsync(string departmentCode, string sectionCode, string machineName, string frequency, CancellationToken cancellationToken = default);

    // Items Sorting grid
    Task<IReadOnlyCollection<CheckSheetGridRowDto>> GetGridAsync(
        string departmentCode,
        string sectionCode,
        string machineName,
        string frequency,
        DateTime checkingDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DateTime>> GetCheckedDatesAsync(
        string departmentCode,
        string sectionCode,
        string machineName,
        string frequency,
        CancellationToken cancellationToken = default);

    Task<Result<CheckSheetGridRowDto>> SaveRowAsync(SaveCheckSheetRowRequest request, CancellationToken cancellationToken = default);
}
