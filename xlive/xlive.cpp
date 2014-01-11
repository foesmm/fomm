#define WIN32_LEAN_AND_MEAN
#include <windows.h>

static char Profile[33];
static DWORD ProfileLen;

//651: int __stdcall XNotifyGetNext(void *, int, int, int)
int __stdcall XNotifyGetNext(void*,DWORD,DWORD,DWORD) { return -1; }

//652: int __stdcall XNotifyPositionUI(LONG Value)
DWORD __stdcall XNotifyPositionUI(DWORD Value) { return -1; }

//1082: __stdcall XGetOverlappedExtendedError(x)
DWORD __stdcall XGetOverlappedExtendedError(void*) { return 0; }

//1083: __stdcall XGetOverlappedResult(x, x, x)
DWORD __stdcall XGetOverlappedResult(void*,DWORD* a,DWORD) {
	if(a) *a=0;
	return 0;
}

//5000: long __stdcall XLiveInitialize(struct _XLIVE_INITIALIZE_INFO *)
int __stdcall XLiveInitialize(void*) { return -1; }

//5001: __stdcall XLiveInput(x)
DWORD __stdcall XLiveInput(DWORD* a) {
	a[5]=0;
	return -1;
}

//5002: int __stdcall XLiveRender()
DWORD __stdcall XLiveRender() { return 0; }

//5022: __stdcall XLiveGetUpdateInformation(x)
DWORD __stdcall XLiveGetUpdateInformation(DWORD) { return -1; }

//5024: int __stdcall XLiveUpdateSystem(LPCWSTR lpString)
int __stdcall XLiveUpdateSystem(const wchar_t* lpString ) { return -1; }

//5030: int __stdcall XLivePreTranslateMessage(HIMC)
int __stdcall XLivePreTranslateMessage(HIMC) { return 0; }

//5215: __stdcall XShowGuideUI(x)
DWORD __stdcall XShowGuideUI(DWORD) { return -1; }

//5250: __stdcall XShowAchievementsUI(x)
DWORD __stdcall XShowAchievementsUI(DWORD) { return -1; }

//5260: __stdcall XShowSigninUI(x, x)
DWORD __stdcall XShowSigninUI(DWORD,DWORD) { return -1; }

//5262: __stdcall XUserGetSigninState(x)
DWORD __stdcall XUserGetSigninState(DWORD) {
	return ProfileLen?1:-1;
} //1 local

//5263: __stdcall XUserGetName(x, x, x)
DWORD __stdcall XUserGetName(DWORD,char* buf,DWORD blen) {
	if(!ProfileLen||blen<ProfileLen) return -1;
	for(DWORD i=0;i<ProfileLen;i++) {
		buf[i]=Profile[i];
	}
	return 0;
}

//5267: __stdcall XUserGetSigninInfo(x, x, x)
DWORD __stdcall XUserGetSigninInfo(DWORD,DWORD local,DWORD* ptr) {
	if(local==2&&ProfileLen) *ptr=1; else *ptr=0;
	return 0;
}

//5270: __stdcall XNotifyCreateListener(x, x)
DWORD __stdcall XNotifyCreateListener(DWORD,DWORD) { return -1; }

//5278: public xlive_5278
DWORD __stdcall f5278(DWORD,DWORD,DWORD) { return -1; }

//5292: __stdcall XUserSetContextEx(x, x, x, x)
DWORD __stdcall XUserSetContextEx(DWORD,DWORD,DWORD,DWORD) { return -1; }

//5293: __stdcall XUserSetPropertyEx(x, x, x, x, x)
DWORD __stdcall XUserSetPropertyEx(DWORD,DWORD,DWORD,DWORD,DWORD) { return -1; }

//5310: int __stdcall TitleExport_XOnlineStartup()
int __stdcall XOnlineStartup() { return 0; }

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//XX new functions used in 1.1 XX
//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

//5251: int __stdcall XCloseHandle(HANDLE hObject)
int __stdcall XCloseHandle(HANDLE hObject) { return 0; }

//5256: int __stdcall XEnumerate(void *, int, int, int, int)
int __stdcall XEnumerate(void *, int, int, int, int) { return -1; }

//5297: int __stdcall XLiveInitializeEx(void *, int)
int __stdcall XLiveInitializeEx(void *, int) { return -1; }

//5355: int __stdcall XLiveContentGetPath(int, struct _XLIVE_CONTENT_INFO_V1 *, unsigned __int16 *, unsigned __int32 *)
int __stdcall XLiveContentGetPath(int, void *, unsigned __int16 *, unsigned __int32 *) { return -1; }

//5356: int __stdcall XLiveContentGetDisplayName(int, struct _XLIVE_CONTENT_INFO_V1 *, unsigned __int16 *, unsigned __int32 *)
int __stdcall XLiveContentGetDisplayName(int, void *, unsigned __int16 *, unsigned __int32 *) { return -1; }

//5360: int __stdcall XLiveContentCreateEnumerator(unsigned __int32, struct _XLIVE_CONTENT_RETRIEVAL_INFO_V1 *, unsigned __int32 *, int)
int __stdcall XLiveContentCreateEnumerator(unsigned __int32, void *, unsigned __int32 *, int) { return -1; }

//5361: int __stdcall XLiveContentRetrieveOffersByDate(int, int, int, int, int, DWORD dwData)
int __stdcall XLiveContentRetrieveOffersByDate(int, int, int, int, int, DWORD dwData) {	return -1; }

//5365: __stdcall XShowMarketplaceUI(x, x, x, x, x)
DWORD __stdcall XShowMarketplaceUI(DWORD,DWORD,DWORD,DWORD,DWORD) {
	return -1;
}

void xliveInit() {
	ProfileLen=GetPrivateProfileStringA("xlive", "profile", "", Profile, 33, ".\\xlive.ini");
	if(ProfileLen>32) ProfileLen=0;
	else if(ProfileLen) ProfileLen++;
}
