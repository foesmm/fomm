#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include "d3dx.h"
#include <d3dx9.h>
#include "SafeWrite.h"

/*
speed test: 1<<16 iterations

Vec3Normalize
sse4: 0e5740
sse3: 124210 (Side effect: Reduced accuracy, requires 8 byte alignment?)
sse2: 1732e0 (Side effect: Doesn't handle null vectors gracefully)
real: 1b76b8 (sse2)

MatrixMultiply
477808
sse2: 2ef788
real: 311ee0 (sse2)

MatrixMultiplyTranspose
sse2: 312968
real: 3642d8 (sse2)

Vec4Transform
sse2: 174f98
real: 1be5e8 (sse2)

Can optimize if matrix is symettric

Vec3TransformNormal
sse2: 1741e8
real: 199eb0

Can optimize if matrix is symettric

PlaneNormalize
sse4: 1113c8
sse3: 160220 (Side effect: Reduced accuracy)
sse2: 1a0210 (Side effect: Doesn't handle null planes gracefully)
real: 1fe638 (sse2)

Vec4Dot
sse4: 0dc578
sse3: 10acc0
real: 1c3ee8 - 0b0200 (inlined x87, depends on how well optimized the inline was)

Would have to manually insert the sse code inline for this one. Far more trouble than it's worth...

MatrixInverse
real: 6538d8

MatrixTranspose
      In place | Copy
x86b: 13fdd8   | NA     (Only used where we know beforehand both arguments are the same)
x86:  147e98   | 1b17d8 (Doesn't support the input and output matricies overlapping)
sse4: 1fd1a8   | 200538
real: 228ad8   | 284100 (x87)

Why does default d3dx use x87 if plain x86 is faster?
*/

static void _declspec(naked) _stdcall sse2MatrixMultiply(D3DXMATRIX*,const D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov     edx, [esp+0x8];
		mov     ecx, [esp+0xc];
		mov     eax, [esp+0x4];
		movlps  xmm0, qword ptr [ecx];
		movhps  xmm0, qword ptr [ecx+8];
		movlps  xmm1, qword ptr [ecx+10h];
		movhps  xmm1, qword ptr [ecx+18h];
		movlps  xmm2, qword ptr [ecx+20h];
		movhps  xmm2, qword ptr [ecx+28h];
		movlps  xmm3, qword ptr [ecx+30h];
		movhps  xmm3, qword ptr [ecx+38h];
		movss   xmm4, dword ptr [edx];
		movss   xmm5, dword ptr [edx+4];
		movss   xmm6, dword ptr [edx+8];
		movss   xmm7, dword ptr [edx+0Ch];
		shufps  xmm4, xmm4, 0;
		shufps  xmm5, xmm5, 0;
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm4, xmm0;
		mulps   xmm5, xmm1;
		mulps   xmm6, xmm2;
		mulps   xmm7, xmm3;
		addps   xmm4, xmm5;
		addps   xmm6, xmm7;
		addps   xmm4, xmm6;
		movss   xmm5, dword ptr [edx+10h];
		movss   xmm6, dword ptr [edx+14h];
		movss   xmm7, dword ptr [edx+18h];
		shufps  xmm5, xmm5, 0;
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm5, xmm0;
		mulps   xmm6, xmm1;
		mulps   xmm7, xmm2;
		addps   xmm5, xmm6;
		addps   xmm5, xmm7;
		movss   xmm6, dword ptr [edx+1Ch];
		shufps  xmm6, xmm6, 0;
		mulps   xmm6, xmm3;
		addps   xmm5, xmm6;
		movss   xmm6, dword ptr [edx+20h];
		movss   xmm7, dword ptr [edx+24h];
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm6, xmm0;
		mulps   xmm7, xmm1;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+28h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm7, xmm2;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+2Ch];
		shufps  xmm7, xmm7, 0;
		mulps   xmm7, xmm3;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+30h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm0, xmm7;
		movss   xmm7, dword ptr [edx+34h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm1, xmm7;
		movss   xmm7, dword ptr [edx+38h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm2, xmm7;
		movss   xmm7, dword ptr [edx+3Ch];
		shufps  xmm7, xmm7, 0;
		mulps   xmm3, xmm7;
		movlps  qword ptr [eax], xmm4;
		movhps  qword ptr [eax+8], xmm4;
		addps   xmm0, xmm1;
		movlps  qword ptr [eax+10h], xmm5;
		movhps  qword ptr [eax+18h], xmm5;
		addps   xmm2, xmm3;
		movlps  qword ptr [eax+20h], xmm6;
		movhps  qword ptr [eax+28h], xmm6;
		addps   xmm0, xmm2;
		movlps  qword ptr [eax+30h], xmm0;
		movhps  qword ptr [eax+38h], xmm0;
		retn    0xC;
	}
}

static void _declspec(naked) _stdcall sse2MatrixMultiplyTranspose(D3DXMATRIX*,const D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov     edx, [esp+0x8];
		mov     ecx, [esp+0xc];
		mov     eax, [esp+0x4];
		movlps  xmm0, qword ptr [ecx];
		movhps  xmm0, qword ptr [ecx+8];
		movlps  xmm1, qword ptr [ecx+10h];
		movhps  xmm1, qword ptr [ecx+18h];
		movlps  xmm2, qword ptr [ecx+20h];
		movhps  xmm2, qword ptr [ecx+28h];
		movlps  xmm3, qword ptr [ecx+30h];
		movhps  xmm3, qword ptr [ecx+38h];
		movss   xmm4, dword ptr [edx];
		movss   xmm5, dword ptr [edx+4];
		movss   xmm6, dword ptr [edx+8];
		movss   xmm7, dword ptr [edx+0Ch];
		shufps  xmm4, xmm4, 0;
		shufps  xmm5, xmm5, 0;
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm4, xmm0;
		mulps   xmm5, xmm1;
		mulps   xmm6, xmm2;
		mulps   xmm7, xmm3;
		addps   xmm4, xmm5;
		addps   xmm6, xmm7;
		addps   xmm4, xmm6;
		movss   xmm5, dword ptr [edx+10h];
		movss   xmm6, dword ptr [edx+14h];
		movss   xmm7, dword ptr [edx+18h];
		shufps  xmm5, xmm5, 0;
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm5, xmm0;
		mulps   xmm6, xmm1;
		mulps   xmm7, xmm2;
		addps   xmm5, xmm6;
		addps   xmm5, xmm7;
		movss   xmm6, dword ptr [edx+1Ch];
		shufps  xmm6, xmm6, 0;
		mulps   xmm6, xmm3;
		addps   xmm5, xmm6;
		movss   xmm6, dword ptr [edx+20h];
		movss   xmm7, dword ptr [edx+24h];
		shufps  xmm6, xmm6, 0;
		shufps  xmm7, xmm7, 0;
		mulps   xmm6, xmm0;
		mulps   xmm7, xmm1;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+28h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm7, xmm2;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+2Ch];
		shufps  xmm7, xmm7, 0;
		mulps   xmm7, xmm3;
		addps   xmm6, xmm7;
		movss   xmm7, dword ptr [edx+30h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm0, xmm7;
		movss   xmm7, dword ptr [edx+34h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm1, xmm7;
		movss   xmm7, dword ptr [edx+38h];
		shufps  xmm7, xmm7, 0;
		mulps   xmm2, xmm7;
		movss   xmm7, dword ptr [edx+3Ch];
		shufps  xmm7, xmm7, 0;
		mulps   xmm3, xmm7;
		addps   xmm0, xmm1;
		addps   xmm2, xmm3;
		addps   xmm0, xmm2;
		movaps  xmm1, xmm4;
		movaps  xmm2, xmm6;
		unpcklps xmm4, xmm5;
		unpcklps xmm6, xmm0;
		unpckhps xmm1, xmm5;
		unpckhps xmm2, xmm0;
		movlps  qword ptr [eax], xmm4;
		movlps  qword ptr [eax+8], xmm6;
		movhps  qword ptr [eax+10h], xmm4;
		movhps  qword ptr [eax+18h], xmm6;
		movlps  qword ptr [eax+20h], xmm1;
		movlps  qword ptr [eax+28h], xmm2;
		movhps  qword ptr [eax+30h], xmm1;
		movhps  qword ptr [eax+38h], xmm2;
		retn    0xC;
	}
}

static void _declspec(naked) _stdcall sse4Vec3Normalize(D3DXVECTOR3*, const D3DXVECTOR3*) {
	_asm {
		mov      ecx,  [esp+0x8];
		mov      eax,  [esp+0x4];
		movss xmm0, [ecx+8];
		movlhps xmm0, xmm0;
		movlps xmm0, [ecx];
		movaps   xmm1, xmm0;
#if _MSC_VER >= 1500
		dpps     xmm0, xmm0, 0x7f;
#else
		_emit 0x66;
		_emit 0x0f;
		_emit 0x3a;
		_emit 0x40;
		_emit 0xc0;
		_emit 0x7f;
#endif
		rsqrtps  xmm0, xmm0;
		mulps    xmm1, xmm0;
		movlps [eax], xmm1;
		movhlps xmm1, xmm1;
		movss [eax+8], xmm1;
		ret 8;
	}
}

static void _declspec(naked) _stdcall sse3Vec3Normalize(D3DXVECTOR3*, const D3DXVECTOR3*) {
	_asm {
		mov ecx, [esp+0x8];
		mov eax, [esp+0x4];
		movss xmm0, [ecx+8];
		movlhps xmm0, xmm0;
		movlps xmm0, [ecx];
		movaps xmm1, xmm0;
		mulps xmm0, xmm0;
		haddps xmm0, xmm0;
		haddps xmm0, xmm0;
		rsqrtss xmm0, xmm0;
		shufps xmm0, xmm0, 0;
		mulps xmm1, xmm0;
		movlps [eax], xmm1;
		movhlps xmm1, xmm1;
		movss [eax+8], xmm1;
		ret 8;
	}
}
static const DWORD norm_a=0x40400000;
static const DWORD norm_b=0x3f000000;
static void _declspec(naked) _stdcall sse2Vec3Normalize(D3DXVECTOR3*, const D3DXVECTOR3*) {
	_asm {
		mov         ecx, [esp+8];
		mov         eax, [esp+4];
		movss       xmm0,dword ptr [ecx+0];
		movss       xmm1,dword ptr [ecx+4];
		movss       xmm2,dword ptr [ecx+8];
		movaps      xmm3,xmm0;
		movaps      xmm4,xmm1;
		movaps      xmm5,xmm2;
		mulss       xmm0,xmm0;
		mulss       xmm1,xmm1;
		mulss       xmm2,xmm2;
		addss       xmm0,xmm1;
		addss       xmm0,xmm2;
		rsqrtss     xmm6,xmm0;
		movaps      xmm7,xmm6;
		mulss       xmm6,xmm0;
		mulss       xmm6,xmm7;
		movss       xmm1, norm_a;
		mulss       xmm7, norm_b;
		subss       xmm1,xmm6;
		mulss       xmm1,xmm7;
		mulss       xmm3,xmm1;
		mulss       xmm4,xmm1;
		mulss       xmm5,xmm1;
		movss       dword ptr [eax],xmm3;
		movss       dword ptr [eax+4],xmm4;
		movss       dword ptr [eax+8],xmm5;
		ret         8;
	}
}

static void _declspec(naked) _stdcall sse2Vec4Transform(D3DXVECTOR4*, const D3DXVECTOR4*, const D3DXMATRIX*) {
	_asm {
		mov     ecx, [esp+0xc];		//; matrix
		mov     edx, [esp+0x8];		//; source
		mov     eax, [esp+0x4];		//; dest
		movups  xmm0, oword ptr [edx];
		test    cl, 0xF;
		jnz     unaligned;
		movaps  xmm2, oword ptr [ecx+30h];
		movaps  xmm3, oword ptr [ecx+20h];
		movaps  xmm1, xmm0;
		shufps  xmm1, xmm0, 0FFh;
		mulps   xmm1, xmm2;
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 0AAh;
		mulps   xmm2, xmm3;
		movaps  xmm3, oword ptr [ecx+10h];
		addps   xmm1, xmm2;
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 55h;
		mulps   xmm2, xmm3;
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 0;
		movaps  xmm0, oword ptr [ecx];
		jmp     end;
unaligned:
		movlps  xmm1, qword ptr [ecx+30h];
		movhps  xmm1, qword ptr [ecx+38h];
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 0FFh;
		mulps   xmm1, xmm2;
		movlps  xmm2, qword ptr [ecx+20h];
		movhps  xmm2, qword ptr [ecx+28h];
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 0AAh;
		mulps   xmm2, xmm3;
		addps   xmm1, xmm2;
		movlps  xmm2, qword ptr [ecx+10h];
		movhps  xmm2, qword ptr [ecx+18h];
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 55h;
		mulps   xmm2, xmm3;
		movlps  xmm3, qword ptr [ecx];
		movhps  xmm3, qword ptr [ecx+8];
		shufps  xmm0, xmm0, 0;
end:
		mulps   xmm3, xmm0;
		addps   xmm2, xmm3;
		addps   xmm1, xmm2;
		movups  oword ptr [eax], xmm1;
		retn    0xC;
	}
}

static void _declspec(naked) _stdcall sse2Vec3TransformNormal(D3DXVECTOR3*, const D3DXVECTOR3*, const D3DXMATRIX*) {
	_asm {
		mov     ecx, [esp+0xc];		//; matrix
		mov     edx, [esp+0x8];		//; source
		mov     eax, [esp+0x4];		//; dest
		movss xmm0,dword ptr [edx+0];
		movss xmm1,dword ptr [edx+4];
		movss xmm2,dword ptr [edx+8];
		unpcklps xmm0, xmm1;
		movlhps xmm0, xmm2;
		test    cl, 0xF;
		jnz     unaligned;
		movaps  xmm2, oword ptr [ecx+30h];
		movaps  xmm3, oword ptr [ecx+20h];
		movaps  xmm1, xmm0;
		shufps  xmm1, xmm0, 0FFh;
		mulps   xmm1, xmm2;
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 0AAh;
		mulps   xmm2, xmm3;
		movaps  xmm3, oword ptr [ecx+10h];
		addps   xmm1, xmm2;
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 55h;
		mulps   xmm2, xmm3;
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 0;
		movaps  xmm0, oword ptr [ecx];
		jmp     end;
unaligned:
		movlps  xmm1, qword ptr [ecx+30h];
		movhps  xmm1, qword ptr [ecx+38h];
		movaps  xmm2, xmm0;
		shufps  xmm2, xmm0, 0FFh;
		mulps   xmm1, xmm2;
		movlps  xmm2, qword ptr [ecx+20h];
		movhps  xmm2, qword ptr [ecx+28h];
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 0AAh;
		mulps   xmm2, xmm3;
		addps   xmm1, xmm2;
		movlps  xmm2, qword ptr [ecx+10h];
		movhps  xmm2, qword ptr [ecx+18h];
		movaps  xmm3, xmm0;
		shufps  xmm3, xmm0, 55h;
		mulps   xmm2, xmm3;
		movlps  xmm3, qword ptr [ecx];
		movhps  xmm3, qword ptr [ecx+8];
		shufps  xmm0, xmm0, 0;
end:
		mulps   xmm3, xmm0;
		addps   xmm2, xmm3;
		addps   xmm1, xmm2;
		movlps [eax], xmm1;
		movhlps xmm1, xmm1;
		movss [eax+8], xmm1;
		retn    0xC;
	}
}

static void _declspec(naked) _stdcall sse4PlaneNormalize(D3DXPLANE*, const D3DXPLANE*) {
	_asm {
		mov ecx, [esp+0x8];
		mov eax, [esp+0x4];
		movss xmm0, [ecx+8];
		movss xmm3, [ecx+0xc];
		movlhps xmm0, xmm0;
		movlps xmm0, [ecx];
		movaps xmm1, xmm0;
#if _MSC_VER >= 1500
		dpps     xmm0, xmm0, 0x7f;
#else
		_emit 0x66;
		_emit 0x0f;
		_emit 0x3a;
		_emit 0x40;
		_emit 0xc0;
		_emit 0x7f;
#endif
		rsqrtps xmm0, xmm0;
		mulps xmm1, xmm0;
		mulss xmm3, xmm0;
		movlps [eax], xmm1;
		movss [eax+0xc], xmm3;
		movhlps xmm1, xmm1;
		movss [eax+8], xmm1;
		ret 8;
	}
}

static void _declspec(naked) _stdcall sse3PlaneNormalize(D3DXPLANE*, const D3DXPLANE*) {
	_asm {
		mov ecx, [esp+0x8];
		mov eax, [esp+0x4];
		movss xmm0, [ecx+8];
		movss xmm3, [ecx+0xc];
		movlhps xmm0, xmm0;
		movlps xmm0, [ecx];
		movaps xmm1, xmm0;
		mulps xmm0, xmm0;
		haddps xmm0, xmm0;
		haddps xmm0, xmm0;
		rsqrtss xmm0, xmm0;
		shufps xmm0, xmm0, 0;
		mulps xmm1, xmm0;
		mulss xmm3, xmm0;
		movlps [eax], xmm1;
		movss [eax+0xc], xmm3;
		movhlps xmm1, xmm1;
		movss [eax+8], xmm1;
		ret 8;
	}
}
static void _declspec(naked) _stdcall sse2PlaneNormalize(D3DXPLANE*, const D3DXPLANE*) {
	_asm {
		mov     ecx, [esp+0x8];
		mov     eax, [esp+0x4];
		movss   xmm0, dword ptr [ecx+0];
		movss   xmm1, dword ptr [ecx+4];
		movss   xmm2, dword ptr [ecx+8];
		movaps  xmm3, xmm0;
		movaps  xmm4, xmm1;
		movaps  xmm5, xmm2;
		mulss   xmm0, xmm0;
		mulss   xmm1, xmm1;
		mulss   xmm2, xmm2;
		addss   xmm0, xmm1;
		addss   xmm0, xmm2;
		rsqrtss xmm6, xmm0;
		movaps  xmm7, xmm6;
		mulss   xmm6, xmm0;
		mulss   xmm6, xmm7;
		movss   xmm1, norm_a;
		mulss   xmm7, norm_b;
		subss   xmm1, xmm6;
		mulss   xmm1, xmm7;
		mulss   xmm3, xmm1;
		mulss   xmm4, xmm1;
		mulss   xmm5, xmm1;
		mulss   xmm1, dword ptr [ecx+0xC];
		movss   dword ptr [eax], xmm3;
		movss   dword ptr [eax+0x4], xmm4;
		movss   dword ptr [eax+0x8], xmm5;
		movss   dword ptr [eax+0xC], xmm1;
		ret     8;
	}
}

static void _declspec(naked) _stdcall x86MatrixTranspose(D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov eax, [esp+0x4];
		mov ecx, [esp+0x8];
		cmp eax, ecx;
		je inplace;
		mov edx, [ecx+0x00];
		mov [eax+0x00], edx;
		mov edx, [ecx+0x04];
		mov [eax+0x10], edx;
		mov edx, [ecx+0x08];
		mov [eax+0x20], edx;
		mov edx, [ecx+0x0c];
		mov [eax+0x30], edx;
		mov edx, [ecx+0x10];
		mov [eax+0x04], edx;
		mov edx, [ecx+0x14];
		mov [eax+0x14], edx;
		mov edx, [ecx+0x18];
		mov [eax+0x24], edx;
		mov edx, [ecx+0x1c];
		mov [eax+0x34], edx;
		mov edx, [ecx+0x20];
		mov [eax+0x08], edx;
		mov edx, [ecx+0x24];
		mov [eax+0x18], edx;
		mov edx, [ecx+0x28];
		mov [eax+0x28], edx;
		mov edx, [ecx+0x2c];
		mov [eax+0x38], edx;
		mov edx, [ecx+0x30];
		mov [eax+0x0c], edx;
		mov edx, [ecx+0x34];
		mov [eax+0x1c], edx;
		mov edx, [ecx+0x38];
		mov [eax+0x2c], edx;
		mov edx, [ecx+0x3c];
		mov [eax+0x3c], edx;
		ret 8;
inplace:
		mov edx, [eax+0x04];
		mov ecx, [eax+0x10];
		mov [eax+0x04], ecx;
		mov [eax+0x10], edx;
		mov edx, [eax+0x08];
		mov ecx, [eax+0x20];
		mov [eax+0x08], ecx;
		mov [eax+0x20], edx;
		mov edx, [eax+0x0c];
		mov ecx, [eax+0x30];
		mov [eax+0x0c], ecx;
		mov [eax+0x30], edx;
		mov edx, [eax+0x18];
		mov ecx, [eax+0x24];
		mov [eax+0x18], ecx;
		mov [eax+0x24], edx;
		mov edx, [eax+0x1c];
		mov ecx, [eax+0x34];
		mov [eax+0x1c], ecx;
		mov [eax+0x34], edx;
		mov edx, [eax+0x2c];
		mov ecx, [eax+0x38];
		mov [eax+0x2c], ecx;
		mov [eax+0x38], edx;
		ret 8;
	}
}
static void _declspec(naked) _stdcall x86MatrixTransposeInplace(D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov edx, [eax+0x04];
		mov ecx, [eax+0x10];
		mov [eax+0x04], ecx;
		mov [eax+0x10], edx;
		mov edx, [eax+0x08];
		mov ecx, [eax+0x20];
		mov [eax+0x08], ecx;
		mov [eax+0x20], edx;
		mov edx, [eax+0x0c];
		mov ecx, [eax+0x30];
		mov [eax+0x0c], ecx;
		mov [eax+0x30], edx;
		mov edx, [eax+0x18];
		mov ecx, [eax+0x24];
		mov [eax+0x18], ecx;
		mov [eax+0x24], edx;
		mov edx, [eax+0x1c];
		mov ecx, [eax+0x34];
		mov [eax+0x1c], ecx;
		mov [eax+0x34], edx;
		mov edx, [eax+0x2c];
		mov ecx, [eax+0x38];
		mov [eax+0x2c], ecx;
		mov [eax+0x38], edx;
		ret 8;
	}
}
void d3dxInit() {
	void* MatrixMultiply;
	void* MatrixMultiplyTranspose;
	void* Vec3Normalize;
	void* Vec4Transform;
	void* PlaneNormalize;
	void* Vec3TransformNormal;
	void* MatrixTranspose;
	void* MatrixTransposeInplace;

	if(*(DWORD*)0xAFDEE6 != 0x121f1fe8) return;

	DWORD ver=GetPrivateProfileIntA("d3dx", "sse", 0, ".//xlive.ini");

	switch(ver) {
		case 4:
			MatrixMultiply=&sse2MatrixMultiply;
			MatrixMultiplyTranspose=&sse2MatrixMultiplyTranspose;
			Vec3Normalize=&sse4Vec3Normalize;
			Vec4Transform=&sse2Vec4Transform;
			PlaneNormalize=&sse4PlaneNormalize;
			Vec3TransformNormal=&sse2Vec3TransformNormal;
			MatrixTranspose=x86MatrixTranspose;
			MatrixTransposeInplace=x86MatrixTransposeInplace;
			break;
		case 3:
			MatrixMultiply=&sse2MatrixMultiply;
			MatrixMultiplyTranspose=&sse2MatrixMultiplyTranspose;
			Vec3Normalize=&sse3Vec3Normalize;
			Vec4Transform=&sse2Vec4Transform;
			PlaneNormalize=&sse3PlaneNormalize;
			Vec3TransformNormal=&sse2Vec3TransformNormal;
			MatrixTranspose=x86MatrixTranspose;
			MatrixTransposeInplace=x86MatrixTransposeInplace;
			break;
		case 2:
			MatrixMultiply=&sse2MatrixMultiply;
			MatrixMultiplyTranspose=&sse2MatrixMultiplyTranspose;
			Vec3Normalize=&sse2Vec3Normalize;
			Vec4Transform=&sse2Vec4Transform;
			PlaneNormalize=&sse2PlaneNormalize;
			Vec3TransformNormal=&sse2Vec3TransformNormal;
			MatrixTranspose=x86MatrixTranspose;
			MatrixTransposeInplace=x86MatrixTransposeInplace;
			break;
		default:
			return;
	}

	HookCall(0x494B39, MatrixMultiply);
	HookCall(0x8727D4, MatrixMultiply);
	HookCall(0x891A9B, MatrixMultiply);
	HookCall(0x892E0E, MatrixMultiply);
	HookCall(0x894E0C, MatrixMultiply);
	HookCall(0x894EA7, MatrixMultiply);
	HookCall(0x894F37, MatrixMultiply);
	HookCall(0x894F54, MatrixMultiply);
	HookCall(0x895008, MatrixMultiply);
	HookCall(0x8950C3, MatrixMultiply);
	HookCall(0x8950D2, MatrixMultiply);
	HookCall(0xAFDE82, MatrixMultiply);
	HookCall(0xAFE4C4, MatrixMultiply);
	HookCall(0xB2103C, MatrixMultiply);
	HookCall(0xB2C759, MatrixMultiply);
	HookCall(0xB40A5E, MatrixMultiply);
	HookCall(0xB41537, MatrixMultiply);

	HookCall(0x892EA8, MatrixMultiplyTranspose);
	HookCall(0xAFED50, MatrixMultiplyTranspose);

	HookCall(0x87250D, Vec3Normalize);
	HookCall(0xAFB4B5, Vec3Normalize);
	HookCall(0xAFEDE8, Vec3Normalize);
	HookCall(0xB2CABF, Vec3Normalize);
	HookCall(0xB33726, Vec3Normalize);
	HookCall(0xB3E5E7, Vec3Normalize);

	HookCall(0x872500, Vec4Transform);
	HookCall(0x891C35, Vec4Transform);
	HookCall(0x891C6C, Vec4Transform);

	HookCall(0x494B8E, PlaneNormalize);
	HookCall(0xAFE56D, PlaneNormalize);
	HookCall(0xB415DD, PlaneNormalize);

	HookCall(0xAFB4A6, Vec3TransformNormal);
	HookCall(0xAFEDD9, Vec3TransformNormal);
	HookCall(0xB2CAAD, Vec3TransformNormal);
	HookCall(0xB33717, Vec3TransformNormal);
	HookCall(0xB3E5D8, Vec3TransformNormal);

	HookCall(0x494B65, MatrixTranspose); //not in place
	HookCall(0x891AC9, MatrixTransposeInplace);
	HookCall(0x891BFA, MatrixTransposeInplace);
	HookCall(0x891EC8, MatrixTransposeInplace);
	HookCall(0x8924C3, MatrixTransposeInplace);
	HookCall(0x892D48, MatrixTransposeInplace);
	HookCall(0x892F29, MatrixTransposeInplace);
	HookCall(0x894CB4, MatrixTransposeInplace);
	HookCall(0x894D2C, MatrixTransposeInplace);
	HookCall(0x894D99, MatrixTransposeInplace);
	HookCall(0x895162, MatrixTransposeInplace);
	HookCall(0x898E6C, MatrixTranspose); //uncertain
	HookCall(0x898FDE, MatrixTranspose); //uncertain
	//HookCall(0x89907B, MatrixTranspose); //The input and output matricies overlap!
	HookCall(0x89944C, MatrixTranspose); //uncertain
	HookCall(0x89963B, MatrixTranspose); //uncertain
	HookCall(0x8996DB, MatrixTranspose); //uncertain
	HookCall(0xAFDC5E, MatrixTranspose); //uncertain
	HookCall(0xAFDEA1, MatrixTransposeInplace);
	HookCall(0xAFDEE6, MatrixTranspose); //not in place
	HookCall(0xAFE4F0, MatrixTranspose); //not in place
	HookCall(0xB2C778, MatrixTransposeInplace);
	HookCall(0xB34DC1, MatrixTransposeInplace);
	HookCall(0xB40A7D, MatrixTransposeInplace);
	HookCall(0xB40ABB, MatrixTransposeInplace);
	HookCall(0xB41563, MatrixTranspose); //not in place
}

/*
//Can't use these because the original function gets inlined
#if _MSC_VER >= 1500
static float _declspec(naked) _stdcall sse4Vec4Dot(const D3DXVECTOR4*,const D3DXVECTOR4*) {
	_asm {
		mov eax, [esp+4];
		mov ecx, [esp+8];
		movups xmm0, [eax];
		movups xmm1, [ecx];
		dpps xmm0, xmm1, 0xff;
		movss [esp+4], xmm0;
		fld dword ptr [esp+4]
		retn 8;
	}
}
#endif
static float _declspec(naked) _stdcall sse3Vec4Dot(const D3DXVECTOR4*,const D3DXVECTOR4*) {
	_asm {
		mov eax, [esp+4];
		mov ecx, [esp+8];
		movups xmm0, [eax];
		movups xmm1, [ecx];
		mulps xmm0, xmm1;
		haddps xmm0, xmm0;
		haddps xmm0, xmm0;
		movss [esp+4], xmm0;
		fld dword ptr [esp+4]
		retn 8;
	}
}


//A bit pointless, since x86 is faster
static void _declspec(naked) _stdcall sse4MatrixTranspose(D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov eax, [esp+0x4];
		mov ecx, [esp+0x8];
		movups xmm0, [ecx+0x00];
		movups xmm1, [ecx+0x10];
		movups xmm2, [ecx+0x20];
		movups xmm3, [ecx+0x30];
		extractps [eax+0x00], xmm0, 0;
		extractps [eax+0x04], xmm1, 0;
		extractps [eax+0x08], xmm2, 0;
		extractps [eax+0x0c], xmm3, 0;
		extractps [eax+0x10], xmm0, 1;
		extractps [eax+0x14], xmm1, 1;
		extractps [eax+0x18], xmm2, 1;
		extractps [eax+0x1c], xmm3, 1;
		extractps [eax+0x20], xmm0, 2;
		extractps [eax+0x24], xmm1, 2;
		extractps [eax+0x28], xmm2, 2;
		extractps [eax+0x2c], xmm3, 2;
		extractps [eax+0x30], xmm0, 3;
		extractps [eax+0x34], xmm1, 3;
		extractps [eax+0x38], xmm2, 3;
		extractps [eax+0x3c], xmm3, 3;
		ret 8;
	}
}

//Runs about 1/5 of the speed of standard mov's
static void _declspec(naked) _stdcall x86MatrixTransposeInplace(D3DXMATRIX*,const D3DXMATRIX*) {
	_asm {
		mov eax, [esp+0x04];
		mov edx, [eax+0x04];
		xchg [eax+0x10], edx;
		mov [eax+0x04], edx;
		mov edx, [eax+0x08];
		xchg [eax+0x20], edx;
		mov [eax+0x08], edx;
		mov edx, [eax+0x0c];
		xchg [eax+0x30], edx;
		mov [eax+0x0c], edx;
		mov edx, [eax+0x18];
		xchg [eax+0x24], edx;
		mov [eax+0x18], edx;
		mov edx, [eax+0x1c];
		xchg [eax+0x34], edx;
		mov [eax+0x1c], edx;
		mov edx, [eax+0x2c];
		xchg [eax+0x38], edx;
		mov [eax+0x2c], edx;
	}
}
*/
