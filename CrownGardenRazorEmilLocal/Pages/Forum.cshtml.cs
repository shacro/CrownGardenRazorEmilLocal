using CrownGardenRazorEmilLocal.Areas.Identity.Data;
using CrownGardenRazorEmilLocal.Datas;
using CrownGardenRazorEmilLocal.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrownGardenRazorEmilLocal.Pages
{
    public class ForumModel : PageModel
    {
        private readonly AppDbContext _appDbContext;
        private readonly IdentityUserContext _indentityContext;
        public List<PostModel> Posts { get; set; }

        [BindProperty]
        public string PostText { get; set; } = "";

        [BindProperty]
        public List<IFormFile> PostPictures { get; set; }

        [BindProperty]
        public string CommentText { get; set; }

        [BindProperty]
        public int PostId { get; set; }
        public ForumModel(AppDbContext appDbContext, IdentityUserContext indentityContext)
        {
            _appDbContext = appDbContext;
            _indentityContext = indentityContext;
        }
        public void OnGet()
        {
            SetPosts();
        }
        public List<(string, string)> GetCommentsForPost(PostModel post)
        {
            List<PostCommentLinkModel> relevantPostCommentLinks = _appDbContext.PostCommentLinks.Where(pl => pl.PostId == post.Id).ToList();

            List<(string, string)> output = new List<(string, string)>();

            foreach (PostCommentLinkModel postCommentLink in relevantPostCommentLinks)
            {
                (string, string) tuple = (_appDbContext.Comments.FirstOrDefault(comment => comment.Id == postCommentLink.CommentId).Comment, _appDbContext.Comments.FirstOrDefault(comment => comment.Id == postCommentLink.CommentId).UserId);
                output.Add(tuple);
            }

            return output;
        }

        private void SetPosts()
        {
            Posts = _appDbContext.Posts.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            SetPosts();

            if (User.Identity.Name == null)
            {
                return Page();
            }

            if (PostPictures != null && PostPictures.Count != 0)
            {
                await UploadPostImage();

                PostModel post = new PostModel
                {
                    PostPic = $"Images/{Path.GetFileName(PostPictures[PostPictures.Count - 1].FileName)}",
                    PostTxt = this.PostText,
                    PostDate = DateTime.Now,
                    IsLiked = false,
                    LikeQuantity = 0,
                    CategoryId = 1,
                    UserId = _indentityContext.Users.FirstOrDefault(user => user.Email == User.Identity.Name).Id
                };

                _appDbContext.Posts.Add(post);
                _appDbContext.SaveChanges();

                SetPosts();
            }
            else if (PostText != "" && PostText != null)
            {
                PostModel post = new PostModel
                {
                    PostTxt = this.PostText,
                    PostDate = DateTime.Now,
                    IsLiked = false,
                    LikeQuantity = 0,
                    CategoryId = 1,
                    UserId = _indentityContext.Users.FirstOrDefault(user => user.Email == User.Identity.Name).Id
                };

                _appDbContext.Posts.Add(post);
                _appDbContext.SaveChanges();

                SetPosts();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostLike()
        {
            if (User.Identity.Name == null)
            {
                return RedirectToPage();
            }



            return RedirectToPage();
        }

        public IActionResult OnPostComment()
        {

            if (User.Identity.Name == null)
            {
                return RedirectToPage();
            }

            if (CommentText == null || CommentText == "")
            {
                return RedirectToPage();
            }

            CommentModel comment = new CommentModel { Comment = CommentText, CommentPostDate = DateTime.Now, UserId = _indentityContext.Users.FirstOrDefault(user => user.Email == User.Identity.Name).Id };
            _appDbContext.Comments.Add(comment);
            _appDbContext.SaveChanges();

            PostCommentLinkModel postCommentLink = new PostCommentLinkModel { PostId = this.PostId, CommentId = comment.Id, };
            _appDbContext.PostCommentLinks.Add(postCommentLink);
            _appDbContext.SaveChanges();

            CommentText = "";

            SetPosts();

            return RedirectToPage(); // för att tömma commentar fältet efter post
        }

        private async Task UploadPostImage()
        {
            string imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            string fileName = Path.GetFileName(PostPictures[PostPictures.Count - 1].FileName);
            var filePath = Path.Combine(imageFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await PostPictures[PostPictures.Count - 1].CopyToAsync(stream);
            }
        }

        public string GetEmailForPost(string UserId)
        {
            return _indentityContext.Users.FirstOrDefault(user => user.Id == UserId).Email;
        }
    }
}
