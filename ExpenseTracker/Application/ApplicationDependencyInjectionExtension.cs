using FluentValidation;

namespace ExpenseTracker.Application;

public static class ApplicationDependencyInjectionExtension
{
    extension(IServiceCollection service)
    {
        public IServiceCollection RegisterApplication()
        {
            service.AddValidatorsFromAssembly(typeof(Program).Assembly);
            return service;
        }
    }
}
