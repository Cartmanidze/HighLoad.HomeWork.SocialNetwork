using HighLoad.HomeWork.SocialNetwork.PostService.Events;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Models;
using HighLoad.HomeWork.SocialNetwork.PostService.Requests;
using HighLoad.HomeWork.SocialNetwork.PostService.Responses;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class PostService(
    IPostRepository postRepository,
    IFeedCacheService feedCacheService,
    IFriendRepository friendRepository, 
    IRabbitMqPublisher rabbitMqPublisher)
    : IPostService
{
    public async Task<PostResponse?> GetAsync(Guid postId)
    {
        var post = await postRepository.GetAsync(postId);

        return post == null ? null : Map(post);
    }

    public async Task<Guid> CreateAsync(PostCreateRequest createRequest)
    {
        var post = Map(createRequest);
        
        var created = await postRepository.CreateAsync(post);
        
        await InvalidateFriendsFeed(createRequest.AuthorId);
        
        var evt = new PostCreatedEvent(
            created,
            post.AuthorId,
            post.Content,
            post.CreatedAt
        );

        await rabbitMqPublisher.PublishPostCreatedAsync(evt);
        
        return created;
    }

    public async Task UpdateAsync(PostUpdateRequest updateRequest)
    {
        var post = Map(updateRequest);
        
        await postRepository.UpdateAsync(post);
        
        await InvalidateFriendsFeed(updateRequest.AuthorId);
    }

    public async Task DeleteAsync(Guid postId)
    {
        var existing = await postRepository.GetAsync(postId);
        
        if (existing != null)
        {
            await postRepository.DeleteAsync(postId);
            
            await InvalidateFriendsFeed(existing.AuthorId);
        }
    }

    public async Task<IReadOnlyCollection<PostResponse>> GetFriendsFeedAsync(Guid userId, int limit)
    {
        var friendIds = await friendRepository.GetFriendIdsAsync(userId);

        var posts = await postRepository.GetPostsByAuthorsAsync(friendIds, limit);

        return posts.Select(Map).ToList();
    }
    
    private async Task InvalidateFriendsFeed(Guid authorId)
    {
        var friendIds = await friendRepository.GetFriendIdsAsync(authorId);
        
        foreach (var friendId in friendIds)
        {
            feedCacheService.InvalidateFeed(friendId);
        }
    }

    private PostResponse Map(Post post) =>
        new()
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    
    private Post Map(PostCreateRequest post) =>
        new()
        {
            AuthorId = post.AuthorId,
            Content = post.Content
        };
    
    private Post Map(PostUpdateRequest post) =>
        new()
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            Content = post.Content
        };
}