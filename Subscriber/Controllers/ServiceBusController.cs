using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using System.Net;
using System.Text;
using System.Text.Json;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBusController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ServiceBusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("sendMessage")]
        public async Task<IActionResult> SendMessageToTopic([FromBody] MessageRequest messageRequest)
        {
            var Settings = _configuration.GetSection("Settings").Get<ConnectionStrings>();
            ITopicClient topicClient = new TopicClient(Settings.ServiceBusConnectionString,Settings.TopicName);
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(messageRequest.Message)));
                message.UserProperties.Add("type", messageRequest.ActionType);
                await topicClient.SendAsync(message);
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                await topicClient.CloseAsync();
            }
        }
    }
}
