using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.DataSet.GetAll;

public record GetDataSetsQuery() : IRequest<IEnumerable<DataSetDto>>;

