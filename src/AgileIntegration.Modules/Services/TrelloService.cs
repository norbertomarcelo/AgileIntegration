using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Interfaces;

namespace AgileIntegration.Modules.Services;

public class TrelloService : ICommonServices
{
    public Task<CreateTaskOutput> CreateTask(CreateTaskInput input)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TaskAlreadyExists(CreateTaskInput input)
    {
        throw new NotImplementedException();
    }

    public bool ValidateTask(CreateTaskInput input, out string errorMessage)
    {
        throw new NotImplementedException();
    }
}
