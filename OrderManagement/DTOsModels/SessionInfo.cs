using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.DTOsModels
{
    public class SessionInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime Expiry { get; set; }
    }
}