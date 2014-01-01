#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include "codepatches.h"
#include "safewrite.h"

static void _declspec(naked) sub_D7BFC0() {
	_asm {
		movaps xmm0, ds:[0x105D2C0];
		movaps ds:[0x1166080], xmm0;
		movaps xmm0, ds:[0x105D2D0];
		movaps ds:[0x1166090], xmm0;
		movaps xmm0, ds:[0x105D2E0];
		movaps ds:[0x11660A0], xmm0;
		ret;
	}
}
static void _declspec(naked) sub_D82AC0() {
	_asm {
		xorps xmm0, xmm0;
		movaps ds:[0x1173090], xmm0;
		movaps ds:[0x11730A0], xmm0;
		movaps ds:[0x11730B0], xmm0;
		movaps ds:[0x11730C0], xmm0;
		movaps ds:[0x11730D0], xmm0;
		movaps ds:[0x11730E0], xmm0;
		movaps ds:[0x11730F0], xmm0;
		movaps ds:[0x1173100], xmm0;
		movaps ds:[0x1173110], xmm0;
		ret;
	}
}
static void _declspec(naked) sub_D82E20() {
	_asm {
		xorps xmm0, xmm0;
		movlps ds:[0x1173198], xmm0;
		movaps ds:[0x11731A0], xmm0;
		movaps ds:[0x11731B0], xmm0;
		movaps ds:[0x11731C0], xmm0;
		movaps ds:[0x11731D0], xmm0;
		movaps ds:[0x11731E0], xmm0;
		movlps ds:[0x11731F0], xmm0;
		ret;
	}
}
static void _declspec(naked) sub_D834C0() {
	_asm {
		xorps xmm0, xmm0;
		movlps ds:[0x1174CE8], xmm0;
		movaps ds:[0x1174CF0], xmm0;
		movlps ds:[0x1174D00], xmm0;
		ret;
	}
}
static void _declspec(naked) sub_D83500() {
	_asm {
		xorps xmm0, xmm0;
		movlps ds:[0x1174D08], xmm0;
		movaps ds:[0x1174D10], xmm0;
		movlps ds:[0x1174D20], xmm0;
		ret;
	}
}

void codepatchesInit() {
	//SafeWrite32(0xDB0294, (DWORD)&sub_D7BFC0);
	//SafeWrite32(0xDB0C94, (DWORD)&sub_D82AC0);
	//SafeWrite32(0xDB0CBC, (DWORD)&sub_D82E20);
	//SafeWrite32(0xDB0D90, (DWORD)&sub_D834C0);
	//SafeWrite32(0xDB0D94, (DWORD)&sub_D83500);
}