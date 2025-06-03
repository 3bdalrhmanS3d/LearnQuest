using LearnQuestV1.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //Task<int> SaveChangesAsync();
        //Task<int> SaveChanges();
        //Task BeginTransactionAsync();
        //Task CommitTransactionAsync();
        //Task RollbackTransactionAsync();

        IBaseRepo<User> Users { get; }
        IBaseRepo<AccountVerification> AccountVerifications { get; }
        IBaseRepo<RefreshToken> RefreshTokens { get; }
        IBaseRepo<UserVisitHistory> UserVisitHistory { get; }
        IBaseRepo<BlacklistToken> BlacklistTokens { get; }

        IBaseRepo<UserDetail> UserDetails { get; }

        Task<int> SaveAsync();


    }

}
