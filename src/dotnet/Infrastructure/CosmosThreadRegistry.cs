﻿using Azure.Storage.Blobs;
using Domain;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    
    public class CosmosThreadRegistry : IThreadRegistry
    {
        private Container _container;
        
        public CosmosThreadRegistry(Container cosmosDbContainer)
        {
            _container = cosmosDbContainer;
        }

        public async Task<List<Domain.Thread>> GetThreadsAsync(string userId)
        {

            List<Domain.Thread> threads = new List<Domain.Thread>();
            string query = string.Format("SELECT * FROM c WHERE c.userId = '{0}' AND c.type = 'CHAT_THREAD'", userId);
            var queryDefinition = new QueryDefinition(query);
            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = 500
            };

            using (var iterator = _container.GetItemQueryIterator<Domain.Thread>(queryDefinition, requestOptions: queryOptions))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        threads.Add(item);
                    }
                }
            }

            return threads;
        }

        public async Task<bool> DeleteThreadAsync(string threadId)
        {

            var response = await _container.DeleteItemAsync<Domain.Thread>(threadId, new PartitionKey(threadId));
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return false;
            }
            return true;
            
        }

        public async Task<Domain.Thread> CreateThreadAsync(string userId)
        {

            string threadId = Guid.NewGuid().ToString();
            DateTime now = DateTime.Now;
            string threadName = now.ToString("dd MMM yyyy, HH:mm");
            Domain.Thread newThread = new()
            {

                Id = threadId,
                Type = "CHAT_THREAD",
                UserId = userId,
                ThreadName = threadName
            };


            var response = await _container.CreateItemAsync<Domain.Thread>(newThread, new PartitionKey(userId));
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Failed to create a new thread.");
            }
            return response;

        }

        public async Task<List<ThreadMessage>> GetMessagesAsync(string threadId)
        {

            List<ThreadMessage> messages = new List<ThreadMessage>();
            string query = string.Format("SELECT * FROM m WHERE m.threadId = '{0}' ORDER BY m._ts DESC", threadId);
            var queryDefinition = new QueryDefinition(query);
            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = 500
            };

            using (var iterator = _container.GetItemQueryIterator<ThreadMessage>(queryDefinition, requestOptions: queryOptions))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        messages.Add(item);
                    }
                }
            }

            return messages;
        }

        public async Task<bool> PostMessageAsync(string userId, string threadId, string message)
        {
            string messageId = Guid.NewGuid().ToString();
            DateTime now = DateTime.Now;
          
            ThreadMessage newMessage = new()
            {

                Id = messageId,
                Type = "CHAT_MESSAGE",
                ThreadId = threadId,
                UserId = userId,
                Role = "user",
                Content = message
            };

            var response = await _container.CreateItemAsync<ThreadMessage>(newMessage, new PartitionKey(userId));
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Failed to create a new thread.");
            }
            return true;
        }

    }
}
