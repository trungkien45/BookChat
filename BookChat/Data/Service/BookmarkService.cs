using BookChat.Data.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.Data.Service
{
    public interface IBookmarkService
    {
    }
    public class BookmarkService : ServiceBase, IBookmarkService
    {
        public BookmarkService(IDbSessionFactory factory) : base(factory)
        {
        }
    }

}
