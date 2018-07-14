using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Vanguard.ServerManager.Core.Abstractions
{
    public sealed class EntityTransactionError
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public EntityTransactionError(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public static EntityTransactionError CreateUniqueError(string key, object value) => new EntityTransactionError("UniqueCollision", $"Duplicate entry '{value}' for key '{key}'.");

        public static EntityTransactionError CreateNoResultsError() => new EntityTransactionError("NoResults", "The entity transaction did not yield any changes in the database.");
    }

    public class EntityTransactionResult<T>
    {
        public bool Succeeded { get; private set; }
        public IEnumerable<EntityTransactionError> Errors { get; private set; }
        public T Value { get; private set; }

        public static EntityTransactionResult<T>  Success(T result)
        {
            return new EntityTransactionResult<T>
            {
                Succeeded = true,
                Value = result
            };
        }

        public static EntityTransactionResult<T> Failure(params EntityTransactionError[] errors)
        {
            return new EntityTransactionResult<T>
            {
                Succeeded = false,
                Errors = errors
            };
        }
    }

    public interface IEntityService<TEntity, TViewModel>
    {
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<TViewModel>> ToListAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TViewModel>> WhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TViewModel> FindAsync(params object[] keyValues);
        Task<EntityTransactionResult<TViewModel>> CreateAsync(TViewModel input, CancellationToken cancellationToken = default);
        Task<EntityTransactionResult<TViewModel>> UpdateAsync(TViewModel input, CancellationToken cancellationToken = default);
        Task<EntityTransactionResult<TViewModel>> DeleteAsync(TViewModel input, CancellationToken cancellationToken = default);
        Task<TViewModel> ToViewModelAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}