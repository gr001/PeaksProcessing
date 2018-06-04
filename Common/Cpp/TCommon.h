#pragma once
#include <algorithm>
//#include "..\vld\include\vld.h"

#ifdef _DEBUG
	#ifndef TASSERT
		#define TASSERT(expr)	_ASSERT((expr))
	#endif
#else
	#ifndef TASSERT
		#define TASSERT(expr)	__noop
	#endif
#endif

#pragma warning( disable : 4251 )	// needs to have dll-interface to be used by clients of class ...

#pragma warning( error : 4258  )// 'variable' : definition from the for loop is ignored; the definition from the enclosing scope is used"
#pragma warning( error : 4789  )// destination of memory copy is too small
#pragma warning( error : 6244  )// local declaration of <variable> hides previous declaration at <line> of <file>
#pragma warning( error : 4316  )// Object allocated on the heap may not be aligned for this type.
#pragma warning( error : 4620  )// There is no postfix increment operator defined for the given type. The compiler used the overloaded prefix operator.
#pragma warning( error : 4715 )	// not all control paths return a value
#pragma warning( error : 4700 )	// uninitialized local variable 'name' used
#pragma warning( error : 4717 ) // recursive on all control paths, function will cause runtime stack overflow
#pragma warning( error : 4150 ) // deletion of pointer to incomplete type 'type'; no destructor called. The delete operator is called to delete a type that was declared but not defined, so the compiler cannot find a destructor.
#pragma warning( error : 4129 ) // unrecognized character escape sequence
#pragma warning( error : 4172 ) // returning address of local variable or temporary


#ifdef _WIN64
	typedef unsigned __int64 TUINT;
	typedef __int64 TINT;
#else
	typedef unsigned __int32 TUINT;
	typedef __int32 TINT;
#endif

#define SAFE_DELETE(x)	{ if (x) { delete x; x = nullptr; } }
#define SAFE_DELETE_ARRAY(x)  { if (x) { delete [] x; x = nullptr; } }

#define GDIPVER     0x0110  // Use more advanced GDI+ features