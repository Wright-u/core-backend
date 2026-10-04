using Wright.Judge.DTOs;

namespace Wright.Judge.Interfaces;

public interface IJudgeService
{
    Task<JudgeResponse> Judge(JudgeRequest request);
}