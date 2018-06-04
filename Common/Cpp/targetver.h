
#pragma once

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers

#define NOMINMAX

#ifndef STRICT
#define STRICT
#endif

#ifdef WINAPI_FAMILY

#include <SDKDDKVer.h>

#else

// The following macros define the minimum required platform.  The minimum required platform
// is the earliest version of Windows, Internet Explorer etc. that has the necessary features to run 
// your application.  The macros work by enabling all features available on platform versions up to and 
// including the version specified.

// Modify the following defines if you have to target a platform prior to the ones specified below.
// Refer to MSDN for the latest info on corresponding values for different platforms.
#ifndef WINVER							// Specifies that the minimum required platform is Windows 7.
#define WINVER _WIN32_WINNT_WIN8   //_WIN32_WINNT_WIN7		//0x0601
#endif

#ifndef _WIN32_WINNT						// Specifies that the minimum required platform is Windows 7.
#define _WIN32_WINNT _WIN32_WINNT_WIN8//_WIN32_WINNT_WIN7		//0x0601
#endif

#ifndef _WIN32_WINDOWS						// Specifies that the minimum required platform is Windows 7.
#define _WIN32_WINDOWS _WIN32_WINNT_WIN8//_WIN32_WINNT_WIN7		//0x0601
#endif

#ifndef _WIN32_IE						// Specifies that the minimum required platform is Internet Explorer 7.0.
#define _WIN32_IE _WIN32_IE_WIN8//0x0700				// Change this to the appropriate value to target other versions of IE.
#endif

#ifndef NTDDI_VERSION
#define NTDDI_VERSION NTDDI_WIN8//NTDDI_WIN7		//(0x06010000)
#endif

#define _AFX_NO_DB_SUPPORT	// No MFC ODBC database classes
#define _AFX_NO_DAO_SUPPORT // No MFC DAO database classes
#define _AFX_NO_OLE_SUPPORT	// No MFC support for Internet Explorer 4 Comon Controls

/*
#if defined(_X86_)
    // the target architecture is x86
#elif defined(_IA64_)
    // the target architecture is Intel ia64
#elif defined(_AMD64_)
    // the target architecture is AMD x86-64
#else
    // some other architecture
#endif
*/


/*
	VC_EXTRALEAN defines the following in AFXV_W32.h:

	WIN32_EXTRA_LEAN
	NOSERVICE
	NOMCX
	NOIME
	NOSOUND
	NOCOMM
	NOKANJI
	NORPC
	NOPROXYSTUB
	NOIMAGE
	NOTAPE
				
	check also Windows.h
*/				

// Including SDKDDKVer.h defines the highest available Windows platform.

// If you wish to build your application for a previous Windows platform, include WinSDKVer.h and
// set the _WIN32_WINNT macro to the platform you wish to support before including SDKDDKVer.h.

//
#include <WinSDKVer.h>
#endif