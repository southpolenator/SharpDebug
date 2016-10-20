// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include <string>
#include <vector>
#include <list>
#include <memory>
#include <map>
#include <chrono>
#include <thread>



#ifdef _WIN32
  #define NO_INLINE __declspec(noinline)
#else
  #define NO_INLINE
#endif
