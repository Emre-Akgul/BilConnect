﻿using BilConnect.Data.Services;
using BilConnect.Data.Static;
using BilConnect.Data.ViewModels;
using BilConnect.Models;
using Bilconnect_First_Version.data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace BilConnect.Controllers
{
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]

    public class ChatsController : Controller
    {
        private readonly IChatsService _service;
        public ChatsController(IChatsService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> Create(int postId, string postOwner)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            // Users should not send messages to themselves
            if (currentUserId == postOwner) {
                return Json(new { success = false, message = "Invalid operation" });
            }
            var allChats = await _service.GetAllChatsAsync();

            // Filter the chats so that only the user's chats will be shown
            var userChats = allChats.Where(c => c.UserId == currentUserId || c.ReceiverId == currentUserId);

            // Check if the chat already exists between two users for a specific post
            bool isDuplicate = false;
            foreach (Chat c in userChats)
            {
                if (c.RelatedPostId == postId)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                ChatVM chat = new ChatVM
                {
                    RelatedPostId = postId,
                    ReceiverId = postOwner,
                    UserId = currentUserId
                };

                await _service.AddNewChatAsync(chat);
            }
            // Success
            return Json(new { success = true, message = "Data saved successfully" });
        }
        public async Task<IActionResult> Index(int postId, string postOwner)
        {
            // If the chat does not exist, create the chat
            if (!string.IsNullOrEmpty(postOwner))
                await Create(postId, postOwner);
            // If the chat exists, show the chats
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allChats = await _service.GetAllChatsAsync();
            var userChats = allChats.Where(c => c.UserId == currentUserId || c.ReceiverId == currentUserId);
            return View(userChats);
        }
        public async Task<IActionResult> Room(int id)
        {
            var data = await _service.GetChatByIdAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Use a ChatViewer class to show the chat in a perspective of the current user
            ChatViewer cv = new ChatViewer();
            cv.RelatedPost = data.RelatedPost;
            cv.Messages = data.Messages;
            if (currentUserId == data.UserId)
            {
                cv.User = data.User;
                cv.Receiver = data.Receiver;
            }
            else
            {
                cv.User = data.Receiver;
                cv.Receiver = data.User;
            }
            cv.Id = data.Id;
            return View(cv);
        }
    }
}