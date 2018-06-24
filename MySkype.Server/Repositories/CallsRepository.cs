﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;

namespace MySkype.Server.Repositories
{
    public class CallsRepository : ICallsRepository
    {
        private readonly MongoContext _context;

        public CallsRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId)
        {
            var userCalls = await _context.Calls
                .AsQueryable()
                .Where(c => c.ParticipantIds.Contains(userId))
                .ToListAsync();

            return userCalls;
        }

        public async Task AddCallAsync(Call call)
        {
            await _context.Calls.InsertOneAsync(call);
        }
    }
}
