using System;
using System.Collections.Generic;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Requests
{
    public class MarkAsReadRequest
    {
        public IReadOnlyCollection<Guid> MessageIds { get; init; } = Array.Empty<Guid>();
    }
} 