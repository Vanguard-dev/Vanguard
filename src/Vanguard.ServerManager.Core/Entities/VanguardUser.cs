﻿using Microsoft.AspNetCore.Identity;

namespace Vanguard.ServerManager.Core.Entities
{
    public class VanguardUser : IdentityUser
    {
        public ServerNode ServerNode { get; set; }
    }
}