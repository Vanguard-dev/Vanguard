using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vanguard.ServerManager.Core.Abstractions;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Entities;

namespace Vanguard.ServerManager.Core.Services
{
    public class ServerNodeService : IEntityService<ServerNode, ServerNodeViewModel>
    {
        private readonly VanguardDbContext _context;

        public ServerNodeService(VanguardDbContext context)
        {
            _context = context;
        }

        public Task<bool> AnyAsync(Expression<Func<ServerNode, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return _context.ServerNodes.AnyAsync(predicate, cancellationToken);
        }

        public async Task<IEnumerable<ServerNodeViewModel>> ToListAsync(CancellationToken cancellationToken = default)
        {
            return await Task.WhenAll((await _context.ServerNodes.ToListAsync(cancellationToken)).Select(t => ToViewModelAsync(t, cancellationToken)));
        }

        public async Task<IEnumerable<ServerNodeViewModel>> WhereAsync(Expression<Func<ServerNode, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.WhenAll((await _context.ServerNodes.Where(predicate).ToListAsync(cancellationToken)).Select(t => ToViewModelAsync(t, cancellationToken)));
        }

        public async Task<ServerNodeViewModel> FindAsync(params object[] keyValues)
        {
            return await ToViewModelAsync(await _context.ServerNodes.FindAsync(keyValues));
        }

        public async Task<EntityTransactionResult<ServerNodeViewModel>> CreateAsync(ServerNodeViewModel input, CancellationToken cancellationToken = default)
        {
            if (await AnyAsync(t => t.Name == input.Name, cancellationToken))
            {
                return EntityTransactionResult<ServerNodeViewModel>.Failure(EntityTransactionError.CreateUniqueError("Name", input.Name));
            }

            var entity = new ServerNode
            {
                Name = input.Name,
                PublicKey = input.PublicKey
            };

            var result = await _context.ServerNodes.AddAsync(entity, cancellationToken);
            if (await _context.SaveChangesAsync(cancellationToken) == 0)
            {
                return EntityTransactionResult<ServerNodeViewModel>.Failure(EntityTransactionError.CreateNoResultsError());
            }

            return EntityTransactionResult<ServerNodeViewModel>.Success(await ToViewModelAsync(result.Entity, cancellationToken));
        }

        public Task<EntityTransactionResult<ServerNodeViewModel>> UpdateAsync(ServerNodeViewModel input, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<EntityTransactionResult<ServerNodeViewModel>> DeleteAsync(ServerNodeViewModel input, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<ServerNodeViewModel> ToViewModelAsync(ServerNode entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ServerNodeViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                PublicKey = entity.PublicKey
            });
        }
    }
}