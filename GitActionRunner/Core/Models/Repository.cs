﻿namespace GitActionRunner.Core.Models
{
    public class Repository
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public bool HasWorkflows { get; set; }
    }
}

