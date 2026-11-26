namespace Todos.Api.Endpoints.Steps;

/// <summary>A step's title and whether it is ticked.</summary>
internal sealed record TodoStepUpdateRequest(string Title, bool IsDone);
