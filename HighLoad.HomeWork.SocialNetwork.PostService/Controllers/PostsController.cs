using Bogus;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Controllers;

[ApiController]
[Route("posts")]
[Authorize]
public sealed class PostsController(IPostService postService, IFeedCacheService feedCacheService, IFriendRepository friendRepository)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var post = await postService.GetAsync(id);
        if (post == null) return NotFound();
        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest createdRequest)
    {
        var created = await postService.CreateAsync(createdRequest);
        
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePost([FromBody] PostUpdateRequest updateRequest)
    {
        await postService.UpdateAsync(updateRequest);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        await postService.DeleteAsync(id);
        
        return Ok();
    }

    [HttpGet("feed/{userId:guid}")]
    public async Task<IActionResult> GetFeed(Guid userId)
    {
        var feed = await feedCacheService.GetFeedAsync(userId);
        return Ok(feed);
    }
    
    [HttpPost("feed/rebuild/{userId:guid}")]
    public async Task<IActionResult> RebuildFeed(Guid userId)
    {
        await feedCacheService.RebuildCacheAsync(userId);
        return Ok("Feed cache rebuilt");
    }
    
    [HttpPost("generate")]
    public async Task<IActionResult> GeneratePosts(
        [FromQuery] int totalPosts = 100,
        [FromQuery] int maxPostsPerUser = 10)
    {
        if (totalPosts <= 0 || maxPostsPerUser <= 0)
        {
            return BadRequest("totalPosts and maxPostsPerUser must be positive.");
        }

        // 1. Получаем список userId. Предположим, вы хотите максимум 50_000 пользователей
        var userIds = (await friendRepository.GetFriendsAsync(50_000)).ToArray();
        if (userIds.Length == 0)
        {
            return BadRequest("No friends found - cannot generate posts.");
        }

        // 2. Готовим случайный генератор
        var random = new Random();
        var faker = new Faker(); // Bogus, для случайного контента

        var createdCount = 0;

        // 3. Создаём посты, пока не достигнем totalPosts
        //    (Можно разбить на batch, если очень много)
        while (createdCount < totalPosts)
        {
            // Выбираем случайного пользователя
            var userIndex = random.Next(userIds.Length);
            var authorId = userIds[userIndex].UserId;

            // Генерируем 1..maxPostsPerUser постов для этого пользователя
            // (или просто 1 в каждом цикле while — на ваше усмотрение)
            int postsForThisUser = random.Next(1, maxPostsPerUser + 1);
            for (int i = 0; i < postsForThisUser; i++)
            {
                if (createdCount >= totalPosts) break;
                
                var content = faker.Lorem.Sentence(10); // Случайный текст
                var request = new PostCreateRequest
                {
                    AuthorId = authorId,
                    Content = content
                };
                
                // Создаём пост
                await postService.CreateAsync(request);

                createdCount++;
            }
        }

        return Ok($"{createdCount} posts generated successfully");
    }
}