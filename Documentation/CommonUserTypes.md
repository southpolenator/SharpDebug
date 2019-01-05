You should read about [user types](UserTypes.md) before continuing

## Existing (common) user types
Currently implemented common user types:
* [STL](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std)
  * [std::any](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/any.cs)
  * [std::array](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/array.cs)
  * [std::basic_string](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/basic_string.cs)
  * [std::filesystem::path](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/filesystem/path.cs)
  * [std::list](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/list.cs)
  * [std::map](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/map.cs)
  * [std::optional](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/optional.cs)
  * [std::pair](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/pair.cs)
  * [std::shared_ptr](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/shared_ptr.cs)
  * [std::string](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/string.cs)
  * [std::unordered_map](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/unordered_map.cs)
  * [std::variant](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/variant.cs)
  * [std::vector](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/vector.cs)
  * [std::weak_ptr](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/weak_ptr.cs)
  * [std::wstring](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/wstring.cs)
* [OpenCV](../Source/CsDebugScript.CommonUserTypes/NativeTypes/cv)
  * [CvMat](../Source/CsDebugScript.CommonUserTypes/NativeTypes/cv/CvMat.cs)
  * [cv::Mat](../Source/CsDebugScript.CommonUserTypes/NativeTypes/std/Mat.cs)
* [Windows](../Source/CsDebugScript.CommonUserTypes/NativeTypes/Windows)
  * [Heap](../Source/CsDebugScript.CommonUserTypes/NativeTypes/Windows/Heap.cs)
  * [ProcessEnvironmentBlock](../Source/CsDebugScript.CommonUserTypes/NativeTypes/Windows/ProcessEnvironmentBlock.cs)
  * [ThreadEnvironmentBlock](../Source/CsDebugScript.CommonUserTypes/NativeTypes/Windows/ThreadEnvironmentBlock.cs)
* [CLR](../Source/CsDebugScript.CommonUserTypes/CLR)
  * [System.Exception](../Source/CsDebugScript.CommonUserTypes/CLR/System/Exception.cs)
  * [System.String](../Source/CsDebugScript.CommonUserTypes/CLR/System/String.cs)

STL classes are verified against different compilers and STL libraries (VS, GCC, Clang).

Common user types can be used with transformations in [code generation](CodeGen.md).
