
using System.Text;
using FluentValidation.Results;

namespace BruteForce.Application.Models;

public record Response<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? Errors { get; private init; }
    public string? Username { get; private set; }

    private Response(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    private Response(List<ValidationFailure> failures)
    {
        var errors = new StringBuilder();
        failures.ForEach(f => errors.AppendLine(f.ErrorMessage));
        Errors = errors.ToString();
        IsSuccess = false;
    }

    public static Response<T> Success(T data) => new(data);

    public static Response<T> Fail(List<ValidationFailure> failures) => new(failures);

    public Response<T> SetUsername(string username)
    {
        Username = username;
        return this;
    }
}
