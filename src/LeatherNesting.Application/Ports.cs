using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;

namespace LeatherNesting.Application;

public interface IProjectStore
{
    Task SaveAsync(string path, ProjectDocument project, CancellationToken cancellationToken);
    Task<ProjectDocument> LoadAsync(string path, CancellationToken cancellationToken);
}

public interface IClock { DateTimeOffset UtcNow { get; } }

public interface IFileDialogService { Task<string?> SelectDxfAsync(CancellationToken cancellationToken); }

public interface INestingProjectStore
{
    Task SaveAsync(string path, NestingProject project, CancellationToken cancellationToken);
    Task<NestingProject> LoadAsync(string path, CancellationToken cancellationToken);
}
