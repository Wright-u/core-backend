using Microsoft.AspNetCore.Mvc;
using Wright.Judge.DTOs;
using Wright.Judge.Interfaces;

namespace Wright.Judge;

[ApiController]
[Route("api/judge")]
public class JudgeController : ControllerBase
{
    private readonly IJudgeService _judgeService;

    public JudgeController(IJudgeService judgeService)
    {
        _judgeService = judgeService;
    }

    [HttpPost]
    public async Task<ActionResult<JudgeResponse>> Judge([FromBody] JudgeRequest request)
    {
        JudgeResponse response = await _judgeService.Judge(request);
        return Ok(response);
    }
}