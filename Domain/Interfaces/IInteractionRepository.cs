using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IInteractionRepository
    {
        Task<Interaction?> GetById(string id);
        Task<List<Interaction?>> GetByBookId(string bookId);

        Task<Interaction?> GetByUserAndBook(string userId, string bookId);
        Task Create(Interaction interaction);
        Task Update(Interaction interaction);

        Task<List<Interaction>> GetByUser(string userId);

        Task Delete(string id);

        Task<List<Interaction>> GetCompletedBooksByUserId(string userId);
        Task<List<Interaction>> GetAllCompletedInPeriod(DateTime startOfMonth, DateTime endOfMonth);
    }
}
