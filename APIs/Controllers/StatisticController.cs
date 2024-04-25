using APIs.Services.Interfaces;
using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.Models.Utils;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [Route("api/statistic")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public StatisticController(IUnitOfWork uow, IMapper mapper)
        {
            _unitOfWork = uow;
            _mapper = mapper;
        }

        //Base CRUD
        [HttpPost("add-stats")]
        public async Task<IActionResult> AddStats(NewStatDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var stats = _mapper.Map<Statistic>(dto);
                    stats.StatId = Guid.NewGuid();
                    stats.LastUpdate = DateTime.Now.Date;
                    await _unitOfWork.StatisticService.AddNewStats(stats);
                    int changes = await _unitOfWork.Save();
                    await transaction.CommitAsync();
                    return Ok(changes);
                }
                catch(Exception e)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("An error occurred while adding stats! Exception: " + e.Message);
                }
            } //transaction.DisposeAsync(); executed here ?
        }

        [HttpGet("get-all-stats")]
        public async Task<IActionResult> GetAllStatRecord()
        {

            return Ok(await _unitOfWork.StatisticService.GetAllAsync());
        }

        [HttpGet("nerd-test")]
        public async Task<IActionResult> GetStatByCate()
        {
            var result = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 6 }
            };

            return Ok(result);
        }

        [HttpGet("get-stats-by-id")]
        public async Task<IActionResult> GetAllStatRecord(Guid id)
        {
            var result = await _unitOfWork.StatisticService.GetStatById(id);
            return (result != null) ? Ok(result) : Ok("No stats found!");
        }

        [HttpPut("update-stats")]
        public async Task<IActionResult> UpdateRecord(UpdateStatDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var record = await _unitOfWork.StatisticService.GetStatById(dto.StatId);

                    if (record == null)
                    {
                        return BadRequest("No stats found!");
                    }

                    var newData = _unitOfWork.StatisticService.CheckNewData(record, dto);
                    newData.LastUpdate = DateTime.Now;

                    //DateTime yesterday = DateTime.UtcNow.AddDays(-1);
                    //if (yesterday.Date < DateTime.UtcNow.Date)
                    //{
                    if (record.LastUpdate.Date != DateTime.Now.Date)
                    {
                        newData.StatId = Guid.NewGuid();
                        await _unitOfWork.StatisticService.AddNewStats(newData);
                    }
                    _unitOfWork.StatisticService.UpdateStats(newData);

                    var result = await _unitOfWork.Save();
                    await transaction.CommitAsync();

                    return (result > 0) ? Ok("Successful!") : Ok("Fail to update stats!");
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                    await transaction.RollbackAsync();
                    return BadRequest("An error occurred while updating stats.");
                }
            }
                  
        }

        [HttpDelete("delete-stats")]
        public async Task<IActionResult> DeleteRecord(Guid statId)
        {
            var record = await _unitOfWork.StatisticService.GetStatById(statId);

            if (record == null)
            {
                return BadRequest("No stats found!");
            }

            _unitOfWork.StatisticService.DeleteStats(statId);

            var result = await _unitOfWork.Save();

            return (result > 0) ? Ok("Successful!") : Ok("Fail to delete stats!");
        }
    }
}

