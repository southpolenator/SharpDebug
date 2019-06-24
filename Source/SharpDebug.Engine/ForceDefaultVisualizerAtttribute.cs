using System;

namespace SharpDebug
{
    /// <summary>
    /// Attribute that tells UI visualizers that we should stop using our visualizers and try to force default visualizers.
    /// If user writes custom visualizer with <see cref="UserType"/> and wants to continue visualizations with
    /// original debugger visualizations (like NatVis in VS) then this property should be marked with this attribute.
    /// Note that in order for this attribute to work, property needs to return <see cref="Variable"/> object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ForceDefaultVisualizerAtttribute : Attribute
    {
    }
}
