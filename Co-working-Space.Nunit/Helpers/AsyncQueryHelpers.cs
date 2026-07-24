using System.Collections;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Co_working_Space.Nunit.Helpers;

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;
    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => (TResult)typeof(Task).GetMethod("FromResult")!
            .MakeGenericMethod(typeof(TResult))
            .Invoke(null, [Execute<TResult>(expression)])!;
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(Expression expression) : base(expression) { }
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    public T Current => inner.Current;
}

internal static class MockAsyncQueryable
{
    public static Mock<IQueryable<T>> BuildAsyncMock<T>(this IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mock = new Mock<IQueryable<T>>();
        mock.Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mock.Setup(x => x.Expression).Returns(queryable.Expression);
        mock.Setup(x => x.ElementType).Returns(queryable.ElementType);
        mock.Setup(x => x.GetEnumerator()).Returns(queryable.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        return mock;
    }

    public static void SetupUsersAsync<TUser>(this Mock<UserManager<TUser>> userManager, IEnumerable<TUser> users) where TUser : class
    {
        var mockQueryable = users.BuildAsyncMock();
        userManager.Setup(x => x.Users).Returns(mockQueryable.Object);
    }
}
