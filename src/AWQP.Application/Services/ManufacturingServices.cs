using AutoMapper;
using AutoMapper.QueryableExtensions;
using AWQP.Application.Common;
using AWQP.Domain.Common;
using AWQP.Domain.Production;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Application.Services;

public sealed class WorkOrderService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<WorkOrderDto>> GetOpenWorkOrdersAsync(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<WorkOrder>()
            .Query()
            .Where(x => !x.IsDeleted && x.Status != WorkOrderStatus.Completed && x.Status != WorkOrderStatus.Cancelled)
            .OrderBy(x => x.ManufacturingDate)
            .ProjectTo<WorkOrderDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var workOrder = new WorkOrder
        {
            WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SalesOrderLineId = request.SalesOrderLineId,
            ProductId = request.ProductId,
            BatchNumber = request.BatchNumber,
            LotNumber = request.LotNumber,
            QuantityPlanned = request.QuantityPlanned,
            Status = WorkOrderStatus.Released
        };

        foreach (var stage in Enum.GetValues<OperationStage>())
        {
            workOrder.Operations.Add(new ManufacturingOperation
            {
                Stage = stage,
                Sequence = (int)stage,
                OperatorUserId = currentUser.UserId,
                QuantityPlanned = request.QuantityPlanned,
                Status = stage == OperationStage.RawMaterialPreparation ? OperationStatus.Queued : OperationStatus.Queued
            });
        }

        await unitOfWork.Repository<WorkOrder>().AddAsync(workOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<WorkOrderDto>(workOrder);
    }

    public async Task<ManufacturingOperationDto> CompleteOperationAsync(CompleteOperationRequest request, CancellationToken cancellationToken)
    {
        var operation = await unitOfWork.Repository<ManufacturingOperation>().GetByIdAsync(request.OperationId, cancellationToken)
            ?? throw new InvalidOperationException("Manufacturing operation not found.");

        operation.EndTime = DateTimeOffset.UtcNow;
        operation.QuantityProduced = request.QuantityProduced;
        operation.QuantityRejected = request.QuantityRejected;
        operation.YieldPercentage = request.QuantityProduced + request.QuantityRejected == 0
            ? 0
            : Math.Round(request.QuantityProduced / (request.QuantityProduced + request.QuantityRejected) * 100, 2);
        operation.Status = OperationStatus.Completed;
        operation.Remarks = request.Remarks;

        var workOrder = await unitOfWork.Repository<WorkOrder>()
            .Query()
            .Include(x => x.Operations)
            .SingleAsync(x => x.Id == operation.WorkOrderId, cancellationToken);

        workOrder.QuantityProduced = workOrder.Operations.Sum(x => x.QuantityProduced);
        workOrder.QuantityRejected = workOrder.Operations.Sum(x => x.QuantityRejected);
        workOrder.YieldPercentage = workOrder.QuantityProduced + workOrder.QuantityRejected == 0
            ? 0
            : Math.Round(workOrder.QuantityProduced / (workOrder.QuantityProduced + workOrder.QuantityRejected) * 100, 2);
        workOrder.Status = workOrder.Operations.All(x => x.Status == OperationStatus.Completed)
            ? WorkOrderStatus.Completed
            : WorkOrderStatus.InProgress;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<ManufacturingOperationDto>(operation);
    }
}
