#define WIN32_LEAN_AND_MEAN
//#define D3D_DEBUG_INFO
#include <d3d9.h>
#include <d3dx9.h>

static IDirect3DDevice9* device;
static IDirect3D9* d3d9;
static ID3DXBuffer* buffer;

#define SAFERELEASE(a) if(a) { a->Release(); a=0; }

//If we're not going to import the standard runtime library, we don't have memset
//this is the easiest implementation that makes sure the compiler doesn't optimize it back to a memset
//Not very efficient, but it's only used on a tiny structure
static void memset2(void* _ptr, int _size) {
	_asm {
		mov ecx, _size;
		mov ebx, _ptr;
		xor eax, eax;
startloop:
		mov [ebx], al;
		inc ebx;
		loop startloop;
	}
}

void _stdcall ddsInit(HWND window) {
	buffer=0;
	d3d9=Direct3DCreate9(D3D_SDK_VERSION);
	D3DPRESENT_PARAMETERS params;
	memset2(&params, sizeof(params));
	params.Windowed=true;
	params.hDeviceWindow=window;
	params.BackBufferHeight=128;
	params.BackBufferWidth=128;
	params.SwapEffect=D3DSWAPEFFECT_DISCARD;
	d3d9->CreateDevice(0, D3DDEVTYPE_NULLREF, 0, D3DCREATE_MULTITHREADED|D3DCREATE_SOFTWARE_VERTEXPROCESSING|D3DCREATE_FPU_PRESERVE, &params, &device);
}

void* _stdcall ddsLoad(BYTE* file, int length) {
	IDirect3DTexture9 *tex=0;
	D3DXIMAGE_INFO info;
	D3DXCreateTextureFromFileInMemoryEx(device, file, length, 0, 0, 1, 0, D3DFMT_A8R8G8B8, D3DPOOL_SCRATCH, D3DX_FILTER_NONE, D3DX_DEFAULT,
		0, &info, 0,  &tex);
	return tex;
}
void* _stdcall ddsCreate(int width, int height) {
	IDirect3DTexture9 *tex=0;
	device->CreateTexture(width, height, 1, 0, D3DFMT_A8R8G8B8, D3DPOOL_SCRATCH, &tex, 0);
	return tex;
}

void _stdcall ddsBlt(IDirect3DTexture9* source, DWORD sL, DWORD sT, DWORD sW, DWORD sH, IDirect3DTexture9* dest, DWORD dL, DWORD dT, DWORD dW, DWORD dH) {
	IDirect3DSurface9 *sourceSurf, *destSurf;
	source->GetSurfaceLevel(0, &sourceSurf);
	dest->GetSurfaceLevel(0, &destSurf);
	RECT sourceRect = { sL, sT, sL+sW, sT+sH };
	RECT destRect = { dL, dT, dL+dW, dT+dH };
	D3DXLoadSurfaceFromSurface(destSurf, 0, &destRect, sourceSurf, 0, &sourceRect, D3DX_DEFAULT, 0);
	sourceSurf->Release();
	destSurf->Release();
}

void* _stdcall ddsSave(IDirect3DTexture9* tex, DWORD format, DWORD mipmaps, DWORD* length) {
	SAFERELEASE(buffer);
	D3DSURFACE_DESC desc;
	tex->GetLevelDesc(0, &desc);
	IDirect3DTexture9* tex2;
	IDirect3DSurface9 *sourceSurf, *destSurf;
	D3DXCreateTexture(device, desc.Width, desc.Height, mipmaps?0:1, 0, (D3DFORMAT)format, D3DPOOL_SCRATCH, &tex2);
	tex->GetSurfaceLevel(0, &sourceSurf);
	tex2->GetSurfaceLevel(0, &destSurf);
	D3DXLoadSurfaceFromSurface(destSurf, 0, 0, sourceSurf, 0, 0, D3DX_DEFAULT, 0);
	sourceSurf->Release();
	destSurf->Release();
	if(mipmaps)	D3DXFilterTexture(tex2, 0, 0, D3DX_DEFAULT);
	D3DXSaveTextureToFileInMemory(&buffer, D3DXIFF_DDS, tex2, 0);
	tex2->Release();
	*length=buffer->GetBufferSize();
	return buffer->GetBufferPointer();
}

void _stdcall ddsRelease(IDirect3DTexture9* tex) {
	tex->Release();
}

void _stdcall ddsGetSize(IDirect3DTexture9* tex, DWORD* width, DWORD* height) {
	D3DSURFACE_DESC desc;
	tex->GetLevelDesc(0, &desc);
	*width=desc.Width;
	*height=desc.Height;
}

void* _stdcall ddsLock(IDirect3DTexture9* tex, DWORD* length, DWORD* pitch) {
	D3DSURFACE_DESC desc;
	tex->GetLevelDesc(0, &desc);
	D3DLOCKED_RECT rect;
	tex->LockRect(0, &rect, 0, D3DLOCK_READONLY);
	*length=rect.Pitch*desc.Height;
	*pitch=rect.Pitch;
	return rect.pBits;
}

void _stdcall ddsUnlock(IDirect3DTexture9* tex) {
	tex->UnlockRect(0);
}

void _stdcall ddsSetData(IDirect3DTexture9* tex, BYTE* data, int length) {
	D3DLOCKED_RECT rect;
	tex->LockRect(0, &rect, 0, 0);
	BYTE* dest=(BYTE*)rect.pBits;
	for(int i=0;i<length;i++) dest[i]=data[i];
	tex->UnlockRect(0);
}

static bool IsPowOfTwo(int i) {
	return i==32||i==64||i==128||i==256||i==512||i==1024||i==2048||i==4086;
}

void* _stdcall ddsShrink(BYTE* file, int length, int* oLength) {
	SAFERELEASE(buffer);
	IDirect3DTexture9 *tex1, *tex2;
	IDirect3DSurface9 *surf1, *surf2;
	D3DXIMAGE_INFO info;
	D3DXCreateTextureFromFileInMemoryEx(device, file, length, 0, 0, 0, 0, D3DFMT_UNKNOWN, D3DPOOL_SCRATCH, D3DX_FILTER_NONE, D3DX_DEFAULT,
		0, &info, 0,  &tex1);
	if(info.MipLevels<3||!IsPowOfTwo(info.Width)||!IsPowOfTwo(info.Height)) {
		tex1->Release();
		return 0;
	}
	device->CreateTexture(info.Width/2, info.Height/2, info.MipLevels-1, 0, info.Format, D3DPOOL_SCRATCH, &tex2, 0);
	for(DWORD i=0;i<info.MipLevels-1;i++) {
		tex1->GetSurfaceLevel(i+1, &surf1);
		tex2->GetSurfaceLevel(i, &surf2);
		D3DXLoadSurfaceFromSurface(surf2, 0, 0, surf1, 0, 0, D3DX_FILTER_NONE, 0);
		surf2->Release();
		surf1->Release();
	}
	D3DXSaveTextureToFileInMemory(&buffer, D3DXIFF_DDS, tex2, 0);
	tex1->Release();
	tex2->Release();
	*oLength=buffer->GetBufferSize();
	return buffer->GetBufferPointer();
}

void _stdcall ddsClose() {
	SAFERELEASE(buffer);
	SAFERELEASE(device);
	SAFERELEASE(d3d9);
}