using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Data.GetDataSets;

public record GetDataSetsQuery() : IRequest<IEnumerable<DataSetDto>>;

