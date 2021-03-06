﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IReceivingDataProvider
    {
        Task<Receiving> GetReceiving(string location, string receivingNo);
        Task<List<Receiving>> GetReceivedItems(string location, string poNo);
        Task<List<Receiving>> GetReceivingSinceLastChecked(string location, DateTime lastChecked);
    }
}
