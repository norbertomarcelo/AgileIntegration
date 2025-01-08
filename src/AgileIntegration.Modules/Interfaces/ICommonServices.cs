using AgileIntegration.Modules.Dtos;

namespace AgileIntegration.Modules.Interfaces;

public interface ICommonServices
{
    public Task<ValidationOutput> TaskAlreadyExists(CreateTaskInput input);
    public ValidationOutput ValidateTask(CreateTaskInput input);
    public Task<CreateTaskOutput> CreateTask(CreateTaskInput input);
}
