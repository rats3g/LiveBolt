using System;

namespace LiveBolt.Models
{
    public class IDM
    {
        public Guid Id { get; set; }
        public bool IsClosed { get; set; }
        public Home AssociatedHome { get; set; }
        public string Nickname { get; set; }
    }
}