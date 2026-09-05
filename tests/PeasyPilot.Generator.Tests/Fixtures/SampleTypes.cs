using PeasyPilot.Generator.Tests.Fixtures.Other;

namespace PeasyPilot.Generator.Tests.Fixtures;

public interface IRepository
{
    string? Find(int id);
}

public enum Status
{
    Active,
    Inactive,
    Pending
}

public class Widget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SampleService
{
    private readonly IRepository _repository;

    public SampleService(IRepository repository)
    {
        _repository = repository;
    }

    public int Add(int a, int b) => a + b;

    public string? FindName(string? query) => query;

    public Status Echo(Status input) => input;

    public List<int> Sum(List<int> numbers) => numbers;

    public Task<Widget?> GetWidgetAsync(int id) => Task.FromResult<Widget?>(null);

    public void DoSomething(Widget widget)
    {
    }

    public ExternalModel Convert(ExternalModel model) => model;
}

public class NoDependencyService
{
    public bool IsPositive(int value) => value > 0;
}
