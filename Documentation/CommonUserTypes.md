You should read about [user types](UserTypes.md) before continuing

## Existing (common) user types
Currently implemented common user types:
* [STL](../Source/SharpDebug.CommonUserTypes/NativeTypes/std)
  * [std::any](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/any.cs)
  * [std::array](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/array.cs)
  * [std::basic_string](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/basic_string.cs)
  * [std::filesystem::path](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/filesystem/path.cs)
  * [std::list](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/list.cs)
  * [std::map](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/map.cs)
  * [std::optional](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/optional.cs)
  * [std::pair](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/pair.cs)
  * [std::shared_ptr](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/shared_ptr.cs)
  * [std::string](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/string.cs)
  * [std::unordered_map](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/unordered_map.cs)
  * [std::variant](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/variant.cs)
  * [std::vector](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/vector.cs)
  * [std::weak_ptr](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/weak_ptr.cs)
  * [std::wstring](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/wstring.cs)
* [OpenCV](../Source/SharpDebug.CommonUserTypes/NativeTypes/cv)
  * [CvMat](../Source/SharpDebug.CommonUserTypes/NativeTypes/cv/CvMat.cs)
  * [cv::Mat](../Source/SharpDebug.CommonUserTypes/NativeTypes/std/Mat.cs)
* [Windows](../Source/SharpDebug.CommonUserTypes/NativeTypes/Windows)
  * [Heap](../Source/SharpDebug.CommonUserTypes/NativeTypes/Windows/Heap.cs)
  * [ProcessEnvironmentBlock](../Source/SharpDebug.CommonUserTypes/NativeTypes/Windows/ProcessEnvironmentBlock.cs)
  * [ThreadEnvironmentBlock](../Source/SharpDebug.CommonUserTypes/NativeTypes/Windows/ThreadEnvironmentBlock.cs)
* [CLR](../Source/SharpDebug.CommonUserTypes/CLR)
  * [System.Exception](../Source/SharpDebug.CommonUserTypes/CLR/System/Exception.cs)
  * [System.String](../Source/SharpDebug.CommonUserTypes/CLR/System/String.cs)

STL classes are verified against different compilers and STL libraries (VS, GCC, Clang).

Common user types can be used with transformations in [code generation](CodeGen.md).
