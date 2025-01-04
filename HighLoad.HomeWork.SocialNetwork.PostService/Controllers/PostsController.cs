using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Requests;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Controllers;

[ApiController]
[Route("posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IFeedCacheService _feedCacheService;

    public PostsController(IPostService postService, IFeedCacheService feedCacheService)
    {
        _postService = postService;
        _feedCacheService = feedCacheService;
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var post = await _postService.GetAsync(id);
        if (post == null) return NotFound();
        return Ok(post);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest createdRequest)
    {
        var created = await _postService.CreateAsync(createdRequest);
        
        return Ok(created);
    }

    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> UpdatePost([FromBody] PostUpdateRequest updateRequest)
    {
        await _postService.UpdateAsync(updateRequest);
        
        return Ok();
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        await _postService.DeleteAsync(id);
        
        return Ok();
    }

    [HttpGet("feed/{userId:guid}")]
    public async Task<IActionResult> GetFeed(Guid userId)
    {
        var feed = await _feedCacheService.GetFeedAsync(userId);
        return Ok(feed);
    }
    
    [HttpPost("feed/rebuild/{userId:guid}")]
    public async Task<IActionResult> RebuildFeed(Guid userId)
    {
        await _feedCacheService.RebuildCacheAsync(userId);
        return Ok("Feed cache rebuilt");
    }
}