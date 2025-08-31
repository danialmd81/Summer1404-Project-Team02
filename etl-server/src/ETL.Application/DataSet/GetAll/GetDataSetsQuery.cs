using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.DataSet.GetAll;

public record GetDataSetsQuery() : IRequest<Result<IEnumerable<DataSetDto>>>;

