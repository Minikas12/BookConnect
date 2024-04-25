using System;
using APIs.Utils.Paging;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;

namespace APIs.Services.Interfaces
{
    public interface IBookGroupService  //DATDQ
    {
        int AddBookGroup(BookGroup bookGroup);
        PagedList<BookGroup> GetAllBookGroup(PagingParams param);
        int UpdateBookGroup(BookGroup bookGroup);
        int DeleteBookGroup(Guid bookGroupId);
        BookGroup GetBookGroupById(Guid bookGroupId);
        string GetOldImgPath(Guid bookGroupId);
        PagedList<BookGroup> GetBookGroupByName(string inputString, PagingParams param);

    }
}