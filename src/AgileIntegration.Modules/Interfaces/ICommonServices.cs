using AgileIntegration.Modules.Dtos;

namespace AgileIntegration.Modules.Interfaces;

public interface ICommonServices
{
    public Task<bool> TaskAlreadyExists(CreateTaskInput input);
    public bool ValidateTask(CreateTaskInput input, out string errorMessage);
    public Task<CreateTaskOutput> CreateTask(CreateTaskInput input);
}
