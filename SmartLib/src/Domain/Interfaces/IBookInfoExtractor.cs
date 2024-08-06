using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLib.src.Domain.Interfaces
{
    public interface IBookInfoExtractor
    {
        Task<BookInfo> GetBookInfoAsync(string filepath);
    }
}
