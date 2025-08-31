using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.UploadCsv;

public record UploadCsvCommand(string TableName, Stream FileStream, string UserId) : IRequest<Result>;
