#define WIN32_LEAN_AND_MEAN
//#define D3D_DEBUG_INFO
#include <d3d9.h>
#include <d3dx9.h>

static IDirect3DDevice9* device;
static IDirect3D9* d3d9;
static ID3DXBuffer* buffer;

void _stdcall ddsInit(HWND window) {
	buffer=0;
	d3d9=Direct3DCreate9(D3D_SDK_VERSION);
	D3DPRESENT_PARAMETERS params;
	memset(&params, 0, sizeof(params));
	params.Windowed=true;
	params.hDeviceWindow=window;
	params.BackBufferHeight=128;
	params.BackBufferWidth=128;
	params.SwapEffect=D3DSWAPEFFECT_DISCARD;
	d3d9->CreateDevice(0, D3DDEVTYPE_NULLREF, 0, D3DCREATE_MULTITHREADED|D3DCREATE_SOFTWARE_VERTEXPROCESSING|D3DCREATE_FPU_PRESERVE, &params, &device);
}

static bool IsPowOfTwo(int i) {
	return i==32||i==64||i==128||i==256||i==512||i==1024||i==2048||i==4086;
}
void* _stdcall ddsSave(BYTE* file, int length, int* oLength) {
	if(buffer) { buffer->Release(); buffer=0; }
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
	device->Release();
	d3d9->Release();
}