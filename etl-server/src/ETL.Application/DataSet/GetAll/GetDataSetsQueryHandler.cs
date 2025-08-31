using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.DataSet.GetAll;

public class GetDataSetsQueryHandler : IRequestHandler<GetDataSetsQuery, Result<IEnumerable<DataSetDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetDataSetsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IEnumerable<DataSetDto>>> Handle(GetDataSetsQuery request, CancellationToken cancellationToken)
    {
        var items = await _uow.DataSets.GetAllAsync(cancellationToken);
        var dataSetDtos = items.Select(d => new DataSetDto(d.Id, d.TableName, d.UploadedByUserId, d.CreatedAt));

        return Result.Success(dataSetDtos);
    }
}
