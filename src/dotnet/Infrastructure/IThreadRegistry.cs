﻿using Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public interface IThreadRegistry
{
    Task<List<Domain.Thread>> GetThreadsAsync(string userId);
    Task<Domain.Thread> CreateThreadAsync(string userId);
    Task<bool> DeleteThreadAsync(string threadId);
    Task<List<ThreadMessage>> GetMessagesAsync(string threadId);
    Task<bool> PostMessageAsync(string userId, string threadId, string message);
}


