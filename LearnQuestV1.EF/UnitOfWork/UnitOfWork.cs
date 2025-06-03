using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.EF.Application;
using LearnQuestV1.EF.Repositories;

namespace LearnQuestV1.EF.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IBaseRepo<User> Users { get; }
        public IBaseRepo<AccountVerification> AccountVerifications { get; }
        public IBaseRepo<RefreshToken> RefreshTokens { get; }
        public IBaseRepo<UserVisitHistory> UserVisitHistories { get; }
        public IBaseRepo<BlacklistToken> BlacklistTokens { get; }
        public IBaseRepo<UserDetail> UserDetails { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new BaseRepo<User>(_context);
            AccountVerifications = new BaseRepo<AccountVerification>(_context);
            RefreshTokens = new BaseRepo<RefreshToken>(_context);
            UserVisitHistories = new BaseRepo<UserVisitHistory>(_context);
            BlacklistTokens = new BaseRepo<BlacklistToken>(_context);
            UserDetails = new BaseRepo<UserDetail>(_context);
        }

        public async Task<int> SaveAsync() =>
            await _context.SaveChangesAsync();

        public void Dispose() =>
            _context.Dispose();

    }
}
