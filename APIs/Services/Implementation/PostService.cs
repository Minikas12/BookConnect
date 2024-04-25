using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using BusinessObjects.Models.Utils;
using DataAccess.DAO;
using DataAccess.DAO.E_com;
using DataAccess.DAO.Trading;

namespace APIs.Services
{
    public class PostService : IPostService
    {
        private readonly PostDAO _postDAO;
        private readonly AccountDAO _accountDAO;
        private readonly CommentOfPostDAO _postCommentDAO;
        private readonly PostInterestDAO _postInterestDAO;
        private readonly CommentDAO _commentDAO;
        private readonly IMapper _mapper;
        private readonly GenericDAO<Statistic> _statDAO;

        public PostService(IMapper mapper)
        {
            _postInterestDAO = new PostInterestDAO();
            _accountDAO = new AccountDAO();
            _postCommentDAO = new CommentOfPostDAO();
            _commentDAO = new CommentDAO();
            _postDAO = new PostDAO();
            _mapper = mapper;
            _statDAO = new GenericDAO<Statistic>(new AppDbContext());
        }

        //---------------------------------------------POST-------------------------------------------------------//

        public async Task<PagedList<Post>> GetAllPostAsync(PagingParams param)
        {
            return PagedList<Post>.ToPagedList((await _postDAO.GetAllPostAsync()).OrderBy(c => c.CreatedAt).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<Post>> GetPostsByUserIdAsync(Guid userId, PagingParams param)
        {
            return PagedList<Post>.ToPagedList((await _postDAO.GetPostsByUserIdAsync(userId)).OrderBy(c => c.CreatedAt).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<Post>> GetPostInterestedInByUserId(Guid userId, PagingParams param)
        {
            return PagedList<Post>.ToPagedList((await _postDAO.GetPostInterestedInByUserIdAsync(userId)).OrderBy(c => c.CreatedAt).AsQueryable(), param.PageNumber, param.PageSize);
        }


        public async Task<int> AddNewPostAsync(Post post) => await _postDAO.AddNewPostAsync(post);

        public async Task<int> DeletePostByIdAsync(Guid postId) => await _postDAO.DeletePostByIdAsync(postId);

        public async Task<Post?> GetPostByIdAsync(Guid postId) => await _postDAO.GetPostByIdAsync(postId);

        public async Task<int> UpdatePostAsync(Post post) => await _postDAO.UpdatePostAsync(post);

        public async Task<string> GetOldImgPathAsync(Guid postId) => await _postDAO.GetOldImgPathAsync(postId);

        public async Task<string> GetOldVideoPathAsync(Guid postId) => await _postDAO.GetOldVideoPathAsync(postId);

        public async Task<bool> IsTradePostAsync(Guid postId) => await _postDAO.IsTradePostAsync(postId);

        public async Task<bool> IsLockedPostAsync(Guid postId) => await _postDAO.IsLockedPostAsync(postId);

        public async Task<int> SetLockPostAsync(bool choice, Guid postId) => await _postDAO.SetLockPostAsync(choice, postId);

        public async Task<bool> IsPostExisted(Guid postId) => await _postDAO.IsPostExisted(postId);

        public string CalculateReadingTime(string content)
        {
            string[] words = content.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //average reading speed of an adult (roughly 265 WPM).
            if(words.Length < 265) return "less than a minute";

            return (int)Math.Ceiling(words.Length / (double)265) + " minutes";
        }

        public async Task<List<PostDetailsDTO>> ConvertToPostDetailsListAsync(PagedList<Post> posts)
        {
            var result = new List<PostDetailsDTO>();

            foreach (var p in posts)
            {
                var user = await _accountDAO.FindUserByIdAsync(p.UserId);
                if (user != null)
                {
                    if (p.Content == null)
                    {
                        p.Content = "";
                    }
                    result.Add(new PostDetailsDTO
                    {
                        PostData = p,
                        ReadingTime = CalculateReadingTime(p.Content),
                        Username = user.Username,
                        AvatarDir = user.AvatarDir,
                        Tags = _mapper.Map<List<CateNameAndIdDTO>>(await GetAllCategoryOfPostAsync(p.PostId))
                    });
                }
            }

            return result;
        }

        //*-----------------------------------------------COMMENT-------------------------------------------------------/*/

        public async Task<PagedList<CommentDetailsDTO>> GetCommentByPostIdAsync(Guid postId, PagingParams @params)
        {
            return PagedList<CommentDetailsDTO>.ToPagedList((await _commentDAO.GetCommentByPostIdAsync(postId))?.OrderBy(c => c.CreateDate).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        public async Task<int> AddCommentAsync(Comment comment) => await _commentDAO.AddCommentAsync(comment);


        //public async Task<int> UpdateCommentAsync(Comment comment) => new CommentDAO().UpdateComment(comment);

        public async Task<int> DeleteCommentByIdAsync(Guid commentId) => await _commentDAO.DeleteCommentByIdAsync(commentId);

        /*-----------------------------------------------POST COMMENT-------------------------------------------------------/*/
        public async Task<int> AddNewCommentRecord(Guid cmtId,Guid postId) => await _postCommentDAO.AddNewRecordAsync(cmtId, postId);
        public async Task<int> DeleteCommentRecord(Guid cmtId) => await _postCommentDAO.DeleteRecordAsync(cmtId);

        /*--------------------------------------------------POST INTEREST-------------------------------------------------------/*/

        public async Task<int> AddNewInteresterAsync(PostInterester postInterest) => await _postInterestDAO.AddNewPostInterestAsync(postInterest);

        public int UpdateInterester(PostInterester postInterest) => _postInterestDAO.UpdatePostInterest(postInterest);

        public async Task<int> DeleteInteresterByIdAsync(Guid postInterestId) => await _postInterestDAO.DeletePostInterestByIdAsync(postInterestId);

        public async Task<PagedList<PostInterester>> GetInteresterByPostIdAsync(Guid postId, PagingParams @params)
        {
            return PagedList<PostInterester>.ToPagedList((await _postInterestDAO.GetPostInterestByPostIdAsync(postId))?.OrderBy(ch => ch.PostInterestId).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        public Task<int> SetIsChosenAsync(bool choice, Guid postInterestId) => _postInterestDAO.SetIsChosenAsync(choice, postInterestId);

        public Task<int> SetIsChosen(bool choice, Guid postInterestId) => _postInterestDAO.SetIsChosenAsync(choice, postInterestId);

        public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId) => await _postDAO.IsPostOwnerAsync(postId, userId);

        public async Task<Guid?> GetLockedRecordIdAsync(Guid traderId, Guid postId) => await _postInterestDAO.GetLockedRecordIdAsync(traderId, postId);

        public async Task<bool> IsAlreadyInterestedAsync(Guid userId, Guid postId) => await _postInterestDAO.IsAlreadyInterestedAsync(userId, postId);

        /*--------------------------------------------------POST BY CATEGORY-------------------------------------------------------/*/
        public async Task<int> AddPostToCategoriesAsync(Guid postId, List<Guid> cateIds)
        => await _postDAO.AddPostToCategoriesAsync(postId, cateIds);

        public async Task<List<Category>> GetAllCategoryOfPostAsync(Guid postId)
        => await _postDAO.GetAllCategoryOfPostAsync(postId);

        public async Task<int> RemovePostfromCategoryAsync(Guid postId, Guid cateId)
        => await _postDAO.RemovePostfromCategoryAsync(postId, cateId);

        public async Task<PagedList<Post>> GetPostByUserTargetedCategoriesAsync(Guid userId, PagingParams @params)
        {
            return PagedList<Post>.ToPagedList((await _postDAO.GetPostByUserTargetedCategoriesAsync(userId)).OrderBy(p => p.CreatedAt).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        ///*------------------------------------User Saved Post------------------------------------*/
        public async Task<int> AddNewUserSavedPostAsync(Guid userId, Guid postId)
        => await _postDAO.AddNewUserSavedPostAsync(userId, postId);

        public async Task<PagedList<Post>> GetUserSavedPostAsync(Guid userId, PagingParams @params)
        {
            return PagedList<Post>.ToPagedList((await _postDAO.GetUserSavedPostAsync(userId)).OrderBy(p => p.CreatedAt).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        public async Task<int> RemoveUserSavedPostAsync(Guid userId, Guid postId)
        => await _postDAO.RemoveUserSavedPostAsync(userId, postId);

        public async Task<bool> IsAlreadySavedAsync(Guid userId, Guid postId)
        => await _postDAO.IsAlreadySavedAsync(userId, postId);
        //public async Task<int> BanPostAsync(Guid postId) => await _postDAO.BanPostAsync(postId);

    }
}
