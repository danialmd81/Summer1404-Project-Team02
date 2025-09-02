using SqlKata;
using SqlKata.Compilers;

namespace ETL.Infrastructure.Data;

public class SqlKataCompilerAdapter : IQueryCompiler
{
    private readonly Compiler _compiler;

    public SqlKataCompilerAdapter(Compiler compiler)
    {
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
    }

    public CompiledQuery Compile(Query query)
    {
        var sqlResult = _compiler.Compile(query);
        return new CompiledQuery(sqlResult.Sql, sqlResult.NamedBindings ?? []);
    }
}
