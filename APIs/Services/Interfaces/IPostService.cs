using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models;
using BusinessObjects.Models.Creative;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using DataAccess.DAO.E_com;

namespace APIs.Services.Interfaces
{
    public interface IPostService
    {
        //---------------------------------------------POST-------------------------------------------------------//

        Task<PagedList<Post>> GetAllPostAsync(PagingParams param);

        Task<PagedList<Post>> GetPostsByUserIdAsync(Guid userId, PagingParams param);

        Task<PagedList<Post>> GetPostInterestedInByUserId(Guid userId, PagingParams param);

        Task<Post?> GetPostByIdAsync(Guid postId);

        Task<int> AddNewPostAsync(Post post);

        Task<int> UpdatePostAsync(Post post);

        Task<int> DeletePostByIdAsync(Guid postId);

        Task<string> GetOldImgPathAsync(Guid postId);

        Task<string> GetOldVideoPathAsync(Guid postId);

        Task<bool> IsTradePostAsync(Guid postId);

        Task<bool> IsLockedPostAsync(Guid postId);

        Task<int> SetLockPostAsync(bool choice, Guid postId);

        Task<bool> IsPostOwnerAsync(Guid postId, Guid userId);

        Task<bool> IsPostExisted(Guid postId);

        string CalculateReadingTime(string content);

        Task<List<PostDetailsDTO>> ConvertToPostDetailsListAsync(PagedList<Post> posts);

        //*-----------------------------------------------COMMENT-------------------------------------------------------/*/

        Task<PagedList<CommentDetailsDTO>> GetCommentByPostIdAsync(Guid postId, PagingParams @params);

        Task<int> AddCommentAsync(Comment comment);

        //public int UpdateComment(Comment comment);

        Task<int> DeleteCommentByIdAsync(Guid commentId);
        /*-----------------------------------------------POST COMMENT-------------------------------------------------------/*/
        Task<int> AddNewCommentRecord(Guid cmtId, Guid postId);
        Task<int> DeleteCommentRecord(Guid cmtId);


        /*--------------------------------------------------POST INTEREST-------------------------------------------------------/*/

        Task<int> AddNewInteresterAsync(PostInterester postInterest);

        int UpdateInterester(PostInterester postInterest);

        Task<int> DeleteInteresterByIdAsync(Guid postInterestId);

        Task<PagedList<PostInterester>> GetInteresterByPostIdAsync(Guid postId, PagingParams @params);

        Task<int> SetIsChosen(bool choice, Guid postInterestId);

        Task<Guid?> GetLockedRecordIdAsync(Guid traderId, Guid postId);

        Task<bool> IsAlreadyInterestedAsync(Guid userId, Guid postId);

        /*--------------------------------------------------POST BY CATEGORY-------------------------------------------------------/*/
        Task<int> AddPostToCategoriesAsync(Guid postId, List<Guid> cateIds);
        Task<List<Category>> GetAllCategoryOfPostAsync(Guid postId);
        Task<int> RemovePostfromCategoryAsync(Guid postId, Guid cateId);
        Task<PagedList<Post>> GetPostByUserTargetedCategoriesAsync(Guid userId, PagingParams @params);

        /*------------------------------------User Saved Post------------------------------------*/
        Task<int> AddNewUserSavedPostAsync(Guid userId, Guid postId);
        Task<PagedList<Post>> GetUserSavedPostAsync(Guid userId, PagingParams @params);
        Task<int> RemoveUserSavedPostAsync(Guid userId, Guid postId);
        Task<bool> IsAlreadySavedAsync(Guid userId, Guid postId);
        //Task<int> BanPostAsync(Guid postId);

    }
}