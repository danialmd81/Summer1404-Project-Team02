using ETL.Application.Abstractions.Data;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Data.GetDataSets;

public class GetDataSetsQueryHandler : IRequestHandler<GetDataSetsQuery, IEnumerable<DataSetDto>>
{
    private readonly IUnitOfWork _uow;

    public GetDataSetsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<DataSetDto>> Handle(GetDataSetsQuery request, CancellationToken cancellationToken)
    {
        var items = await _uow.DataSets.GetAllAsync(cancellationToken);
        return items.Select(d => new DataSetDto(d.Id, d.TableName, d.UploadedByUserId, d.UploadedAt));
    }
}
