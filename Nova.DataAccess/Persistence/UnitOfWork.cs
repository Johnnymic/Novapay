using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nova.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.DataAccess.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NovaWalletDbContext _context;

        private readonly Dictionary<Type, object> _repositories = new();

        private IDbContextTransaction? _transaction;



        public UnitOfWork(NovaWalletDbContext context)
        {
            _context = context;



        }

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.ContainsKey(type))
                return (IRepository<T>)_repositories[type];

            var repository = new Repository<T>(_context);

            _repositories.Add(type, repository);

            return repository;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
                _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task LockWalletAsync(
                  Guid walletId)
        {
            await ExecuteWalletLockAsync(
                walletId);
        }

        public async Task LockWalletsAsync(
            Guid firstWalletId,
            Guid secondWalletId)
        {
            

            if (firstWalletId.CompareTo(secondWalletId) > 0)
            {
                (firstWalletId, secondWalletId) =
                    (secondWalletId, firstWalletId);
            }

            await ExecuteWalletLockAsync(
                firstWalletId);

            await ExecuteWalletLockAsync(
                secondWalletId);
        }

        private async Task ExecuteWalletLockAsync(
            Guid walletId)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "A database transaction must be started before locking a wallet.");
            }

            var connection =
                _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command =
                connection.CreateCommand();

            command.Transaction =
                _transaction.GetDbTransaction();

            command.CommandText = """
                SELECT Id
                FROM Wallets
                WHERE Id = @walletId
                FOR UPDATE
                """;

            var parameter = command.CreateParameter();

            parameter.ParameterName = "@walletId";
            parameter.Value = walletId;

            command.Parameters.Add(parameter);

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new KeyNotFoundException(
                    $"Wallet '{walletId}' was not found.");
            }
        }
    }
}
