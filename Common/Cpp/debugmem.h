#pragma once

//#ifdef _DEBUG
//	#include "..\vld\include\vld.h"
//	#pragma comment(lib, "vld.lib")
//#endif

//#ifdef _DEBUG
//
//#include <crtdbg.h>
//#include <list>
////#include <boost/shared_ptr.hpp>
//#include "..\Others\TShared\Diagnostics\StackWalker.h"
////static std::list<boost::shared_ptr<std::string> > callstacks;
//inline void* __cdecl operator new(size_t nSize, const char* lpszFileName, int nLine)
//{
//	//static StackWalkerToVector stack;
//	//stack.ShowCallstack();
//	//boost::shared_ptr<std::string> str(new std::string());
//
//	//for(size_t i=0; i<stack.CallStack().size(); i++)
//	//{
//	//	str->append(stack.CallStack()[i]);
//	//	(*str) += ' ';
//	//}
//
//	//callstacks.push_back(str);
//	
//	return ::operator new(nSize, _NORMAL_BLOCK, lpszFileName, nLine);
//	//return ::operator new(nSize, _NORMAL_BLOCK, str->c_str(), nLine);
//}
//
//inline void __cdecl operator delete(void* p, const char* lpszFileName, int nLine)
//{
//	::operator delete(p, _NORMAL_BLOCK, lpszFileName, nLine);
//}
//
//#if _MSC_VER >= 1210
//
//inline void* __cdecl operator new[](size_t nSize, const char* lpszFileName, int nLine)
//{
//	return ::operator new[](nSize, _NORMAL_BLOCK, lpszFileName, nLine);
//}
//
//inline void __cdecl operator delete[](void* p, const char* lpszFileName, int nLine)
//{
//	::operator delete[](p, _NORMAL_BLOCK, lpszFileName, nLine);
//}
//
//#endif
//
//#define NEW new(__FILE__, __LINE__)
//
//#else

//#define NEW new

//#endif // _DEBUG
