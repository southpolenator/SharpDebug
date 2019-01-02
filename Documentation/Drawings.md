# Drawing objects
Drawing library help visualize results in [interactive mode](InteractiveMode.md). It can be used during debugging any image processing or computer vision app.

Basic interface used for creating objects is [IGraphics](../Source/CsDebugScript.Drawing.Interfaces/IGraphics.cs). It allows more than just visualizing images, you can visualize detected objects on images while debugging.
In interactive mode, you can get this interface by quering global object `Graphics`.

Dumping drawing object will open drawing visualizer:

// TODO: Insert image here
