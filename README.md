# ProceduralDataflow

This library uses the custom awaiter feature in C# 5 and the task-like feature in C# 7 to enable C# developers to write dataflows in a procedural way.

The dataflow pattern is like the producer-consumer pattern. However, in the dataflow pattern, the flow is not linear. It can contain branches and loops.

The .NET framework already contains APIs to help developers create dataflows. For example, you can use the [Dataflow API](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library). This API is great. However, the way we describe the dataflow in such API is declarative. That is, we create *blocks* and *link* them together. An issue with this approach is that the flow is not easily readable. It would be much easier if we can write the dataflow like this:

    var result1 = RunBlock1(input);
    
    var result2 = RunBlock2(result1);
    
    if(result2.SomeProperty == "some value")
    {
        return RunBlock3(result2.Part1);
    }
    else
    {
        return RunBlock4(result2.Part2);
    }
    
And have the framework know how to connect the blocks together.

This is exactly what ProceduralDataflow does.
