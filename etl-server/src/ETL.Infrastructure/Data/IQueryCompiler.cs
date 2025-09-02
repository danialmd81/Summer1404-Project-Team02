using SqlKata;

namespace ETL.Infrastructure.Data;

public record CompiledQuery(string Sql, object NamedBindings);

public interface IQueryCompiler
{
    CompiledQuery Compile(Query query);
}

