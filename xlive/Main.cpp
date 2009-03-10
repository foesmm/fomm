#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include "d3dx.h"
#include "xlive.h"

DWORD MyMain(DWORD hDllHandle, DWORD dwReason, DWORD  lpreserved) {
	if(dwReason==DLL_PROCESS_ATTACH) {
		DisableThreadLibraryCalls(GetModuleHandle("xlive.dll"));
		d3dxInit();
		xliveInit();
	}
	return 1;
}