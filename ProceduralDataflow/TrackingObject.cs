using System;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralDataflow
{
    public class TrackingObject
    {
        public HashSet<Guid> VisitedNodes { get; } = new HashSet<Guid>();

        public static ThreadLocal<TrackingObject> CurrentProcessingItem = new ThreadLocal<TrackingObject>(() => null);
    }
}