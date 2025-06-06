using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vjezba.Model;
using Vjezba.DAL;
using FirebaseAdmin.Messaging;

namespace Vjezba.Web.Controllers
{
    public class FcmController : Controller
    {
        private readonly ILogger<FcmController> _logger;
        private readonly ClientManagerDbContext _context;

        public FcmController(
            ILogger<FcmController> logger,
            ClientManagerDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Save(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Token is required" });
            }

            try
            {
                var exists = await _context.FcmTokens.AnyAsync(t => t.Token == token);
                if (!exists)
                {
                    var fcmToken = new FcmToken { Token = token };
                    _context.FcmTokens.Add(fcmToken);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving FCM token");
                return Json(new { success = false, message = "Error saving token" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Tokens()
        {
            try
            {
                var tokens = await _context.FcmTokens
                    .Select(t => t.Token)
                    .ToListAsync();

                return Json(new { success = true, tokens });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FCM tokens");
                return Json(new { success = false, message = "Error retrieving tokens" });
            }
        }

        public IActionResult Invite()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite(string lobbyCode)
        {
            if (string.IsNullOrEmpty(lobbyCode))
            {
                return Json(new { success = false, message = "Lobby code is required" });
            }

            try
            {
                var tokens = await _context.FcmTokens
                    .Select(t => t.Token)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    return Json(new { success = false, message = "No FCM tokens found" });
                }

                var successCount = 0;
                var failCount = 0;

                foreach (var token in tokens)
                {
                    try
                    {
                        await SendFcmNotification(token, lobbyCode);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to token: {Token}", token);
                        failCount++;
                    }
                }

                return Json(new { 
                    success = true, 
                    message = $"Sent to {successCount} devices. {failCount} failed." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invites");
                return Json(new { success = false, message = "Error sending invites" });
            }
        }

        private async Task SendFcmNotification(string token, string lobbyCode)
{
    try
    {
        var message = new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = "Game Invite!",
                Body = $"You're invited to join lobby: {lobbyCode}"
            },
            Data = new Dictionary<string, string>()
            {
                { "lobbyCode", lobbyCode }
            }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        _logger.LogInformation("Successfully sent message: {Response}", response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending FCM notification to token: {Token}", token);
        throw;
    }
}
    }
}