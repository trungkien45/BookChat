using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.Models
{
    public class Bookmark: Entity
    {
        public string Name { get; set; } = string.Empty; 
        public int PageNumber { get; set; }
        public int BookId { get; set; }
    }
}
