using Application.Common.Result;
using Application.Dtos.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJournalService
    {
        Task<Result<List<JournalItemDto>>> GetUserJournal();
    }
}
