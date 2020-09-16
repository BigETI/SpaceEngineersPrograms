using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public delegate TResult CommandDelegate<TResult>(IReadOnlyList<string> arguments);

    public delegate TResult CommandDelegate<TResult, TContext>(IReadOnlyList<string> arguments, TContext context);
}
