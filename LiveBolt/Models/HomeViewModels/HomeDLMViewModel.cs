using System;

namespace LiveBolt.Models.HomeViewModels
{
    public class HomeDLMViewModel
    {
        public Guid Id { get; set; }
        public bool IsLocked { get; set; }
        public string Nickname { get; set; }
    }
}