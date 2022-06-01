using System;

namespace Project.Core.Interfaces
{
    public interface IModificationTime
    {
        DateTime? LastModifiedTime { get; set; }
    }
}
