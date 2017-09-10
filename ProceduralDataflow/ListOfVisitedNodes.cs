using System;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralDataflow
{
    public class ListOfVisitedNodes
    {
        public HashSet<Guid> VisitedNodes { get; } = new HashSet<Guid>();

        public static ThreadLocal<ListOfVisitedNodes> Current = new ThreadLocal<ListOfVisitedNodes>(() => null);
    }
}