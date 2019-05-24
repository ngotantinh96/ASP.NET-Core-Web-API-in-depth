using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await campRepository.GetTalksByMonikerAsync(moniker, true);
                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, id, true);
                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await campRepository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                //Create a new camp
                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker Id is required");
                var speaker = await campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");
                talk.Speaker = speaker;

                campRepository.Add(talk);
                await campRepository.SaveChangesAsync();

                var url = linkGenerator
                    .GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });

                return Created(url, mapper.Map<TalkModel>(talk));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var oldTask = await campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (oldTask == null) return NotFound($"Could not find camp with moniker: {moniker} and id: {id}");

                mapper.Map(model, oldTask);

                if (model.Speaker != null)
                {
                    var speaker = await campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                        oldTask.Speaker = speaker;
                }

                await campRepository.SaveChangesAsync();

                return mapper.Map<TalkModel>(oldTask);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var oldTask = await campRepository.GetTalkByMonikerAsync(moniker, id);
                if (oldTask == null) return NotFound($"Could not find camp with moniker: {moniker} and id: {id}");


                campRepository.Delete(oldTask);

                if (await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
            return BadRequest();
        }
    }
}
