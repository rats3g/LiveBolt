using System;

namespace LiveBolt.Models
{
    public class DLM
    {
        public Guid Id { get; set; }
        public bool IsLocked { get; set; }
        public int AssociatedHomeId { get; set; }
        public string Nickname { get; set; }
    }
}