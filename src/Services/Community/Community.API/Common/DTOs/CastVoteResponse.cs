namespace Community.API.Common.DTOs;

public class CastVoteResponse
{
    public Guid TargetId { get; set; }
    public int NewVoteCount { get; set; }
    public int UserVote { get; set; } // 1, -1, or 0 (none)
}
