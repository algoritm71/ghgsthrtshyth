using WebApplication2.Data.Interfaces;
using WebApplication2.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApplication2.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IRepository<Product>? _products;
        private IRepository<Category>? _categories;
        private IRepository<User>? _users;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<Product> Products
        {
            get
            {
                if (_products == null)
                {
                    _products = new Repository<Product>(_context);
                }
                return _products;
            }
        }

        public IRepository<Category> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new Repository<Category>(_context);
                }
                return _categories;
            }
        }

        public IRepository<User> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new Repository<User>(_context);
                }
                return _users;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}