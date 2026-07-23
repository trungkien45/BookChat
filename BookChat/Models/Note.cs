using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.Models
{
    public class Note : Entity
    {
        public string? Content { get; set; }
        public int PageNumber { get; set; }
        public int BookId { get; set; }
    }
}
