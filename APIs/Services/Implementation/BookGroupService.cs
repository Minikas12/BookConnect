using System;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.Models;
using BusinessObjects.Models.Creative;
using BusinessObjects.Models.Utils;
using CloudinaryDotNet.Actions;
using DataAccess.DAO;

namespace APIs.Services
{
    public class BookGroupService : IBookGroupService
    {
        public int AddBookGroup(BookGroup bookGroup) => new BookGroupDAO().AddBookGroup(bookGroup);

        public int DeleteBookGroup(Guid bookGroupId) => new BookGroupDAO().DeleteBookGroupById(bookGroupId);


        public PagedList<BookGroup> GetAllBookGroup(PagingParams param)
        {
            return PagedList<BookGroup>.ToPagedList(new BookGroupDAO().GetAllBookGroup().OrderBy(bg => bg.BookGroupName).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public int UpdateBookGroup(BookGroup bookGroup) => new BookGroupDAO().UpdateBookGroup(bookGroup);

        public BookGroup GetBookGroupById(Guid bookGroupId) => new BookGroupDAO().GetBookGroupById(bookGroupId);

        public string GetOldImgPath(Guid bookGroupId) => new BookGroupDAO().GetOldImgPath(bookGroupId);

        public PagedList<BookGroup> GetBookGroupByName(string inputString, PagingParams param)
        {
            return PagedList<BookGroup>.ToPagedList(new BookGroupDAO().GetBookGroupByName(inputString).OrderBy(bl => bl.BookGroupName).AsQueryable(), param.PageNumber, param.PageSize);
        }
    }
}