using BusinessObjects;
using BusinessObjects.Models.Utils;

namespace DataAccess.DAO
{
    public class BookGroupDAO
    {
        public int AddBookGroup(BookGroup bookGroup)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.BookGroups.Add(bookGroup);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookGroup> GetAllBookGroup()
        {
            try
            {
                List<BookGroup> result = new List<BookGroup>();
                using (var context = new AppDbContext())
                {
                    if (context.BookGroups.Any())
                    {
                        result = context.BookGroups.ToList();
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int UpdateBookGroup(BookGroup bookGroup)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.BookGroups.Update(bookGroup);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int DeleteBookGroupById(Guid bookGroupId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    BookGroup? bookGroup = context.BookGroups.Where(bg => bg.BookGroupId == bookGroupId).SingleOrDefault();

                    if (bookGroup != null)
                    {
                        context.BookGroups.Remove(bookGroup);
                    }
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public BookGroup GetBookGroupById(Guid bookGroupId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    BookGroup? result = context.BookGroups.Where(bg => bg.BookGroupId == bookGroupId).SingleOrDefault();
                    if (result != null)
                    {
                        return result;
                    }
                    return new BookGroup();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetOldImgPath(Guid bookGroupId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    BookGroup? bookGr = context.BookGroups.Where(bg => bg.BookGroupId == bookGroupId).SingleOrDefault();
                    string result = (bookGr != null && bookGr.ImageDir != null) ?
                        bookGr.ImageDir : "";
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookGroup> GetBookGroupByName(string inputString)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookGroup> result = new List<BookGroup>();
                    var matchedCates = context.BookGroups
                    .Where(bg => bg.BookGroupName.Contains(inputString))
                    .ToList();
                    if (matchedCates.Count > 0)
                    {
                        result = matchedCates;
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}

