using LeatherNesting.Application;

namespace LeatherNesting.Infrastructure.Dxf;

public interface IDxfWriter { Task WriteAsync(string path, CancellationToken cancellationToken); }
