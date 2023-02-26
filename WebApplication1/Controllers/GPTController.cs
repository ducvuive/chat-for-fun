using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpenAI_API;
using OpenAI_API.Completions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Hubs;
using WebApplication1.Models;

namespace ChatGPT_CSharp.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class GPTController : ControllerBase
    {
        private readonly ManageAppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMapper _mapper;
        public GPTController(ManageAppDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        [HttpGet]
        [Route("UseChatGPT")]
        public async Task<string> UseChatGPT(string query)
        {
            string OutPutResult = "";
            var openai = new OpenAIAPI("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");
            CompletionRequest completionRequest = new CompletionRequest();
            completionRequest.Prompt = query;
            completionRequest.Model = OpenAI_API.Models.Model.DavinciText;
            completionRequest.MaxTokens = 600;
            completionRequest.Temperature = 0.7;
            completionRequest.TopP = 1.0;
            completionRequest.NumChoicesPerPrompt = 1;
            var completions = openai.Completions.CreateCompletionAsync(completionRequest);

            foreach (var completion in completions.Result.Completions)
            {
                OutPutResult += completion.Text;
            }

            return OutPutResult;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
            return Ok(messageViewModel);
        }

        [HttpPost]
        [Route("CreateMessageAI")]
        public async Task<ActionResult<Message>> CreateMessageAI(MessageViewModel messageViewModel)
        {
            if (messageViewModel.Room == "ChatGPT")
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == "ChatBot");
                var room = _context.Rooms.FirstOrDefault(r => r.Name == "ChatGPT");
                var content = await UseChatGPT(messageViewModel.Content);
                messageViewModel.Content = content;
                if (room == null)
                    return BadRequest();

                var msg = new Message()
                {
                    //Content = Regex.Replace(messageViewModel.Content, @"<.*?>", string.Empty),
                    Content = Regex.Replace(messageViewModel.Content, @"\\n", "<br/>"),
                    FromUser = user,
                    ToRoom = room,
                    Timestamp = DateTime.Now
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();

                // Broadcast the message
                var createdMessage = _mapper.Map<Message, MessageViewModel>(msg);
                await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", createdMessage);

                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("GetRoomChatGPT")]
        public async Task<ActionResult<Room>> GetRoomChatGPT()
        {
            var roomGPT = _context.Rooms.FirstOrDefault(r => r.Name == "ChatGPT");
            if (roomGPT == null)
                return BadRequest();
            var roomsViewModel = _mapper.Map<Room, RoomViewModel>(roomGPT);
            return Ok(roomsViewModel);
        }
    }
}
