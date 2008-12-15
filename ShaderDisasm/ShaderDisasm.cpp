#include <d3d9.h>
#include <d3dx9.h>

char text[0x10000];

typedef HRESULT (_stdcall *DisProc)(CONST DWORD*, BOOL, LPCSTR, LPD3DXBUFFER*);
typedef HRESULT (_stdcall *AsmProc)(LPCSTR, UINT, CONST D3DXMACRO*, LPD3DXINCLUDE, DWORD, LPD3DXBUFFER*, LPD3DXBUFFER*);
typedef HRESULT (_stdcall *CompProc)(LPCSTR, UINT, CONST D3DXMACRO*, LPD3DXINCLUDE, LPCSTR, LPCSTR, DWORD, LPD3DXBUFFER*, LPD3DXBUFFER*, LPD3DXCONSTANTTABLE*);

HMODULE d3dx=0;
DisProc DisassembleShader=0;
AsmProc AssembleShader=0;
CompProc CompileShader=0;

static void Load() {
	d3dx=LoadLibraryA("d3dx9_38.dll");
	DisassembleShader=(DisProc)GetProcAddress(d3dx, "D3DXDisassembleShader");
	AssembleShader=(AsmProc)GetProcAddress(d3dx, "D3DXAssembleShader");
	CompileShader=(CompProc)GetProcAddress(d3dx, "D3DXCompileShader");
}

char* _stdcall Disasm(byte b[],int len,byte col) {
	if(!d3dx) Load();
	ID3DXBuffer* out;
	if(FAILED(DisassembleShader((DWORD*)b,col,0,&out))) return 0;
	for(DWORD i=0;i<out->GetBufferSize();i++) text[i]=((char*)out->GetBufferPointer())[i];
	out->Release();
	return text;
}

char* _stdcall Asm(char* in,int len) {
	if(!d3dx) Load();
	ID3DXBuffer* out;
	ID3DXBuffer* errors;
	if(AssembleShader(in,len,0,0,0,&out,&errors)!=D3D_OK) {
		if(out) out->Release();
		if(errors) {
			((DWORD*)text)[0]=0;
			for(DWORD i=0;i<errors->GetBufferSize();i++) text[i+4]=((char*)errors->GetBufferPointer())[i];
			errors->Release();
		} else {
			((DWORD*)text)[0]=0;
			((DWORD*)text)[1]=0;
		}
	} else {
		if(errors) errors->Release();
		((DWORD*)text)[0]=out->GetBufferSize();
		for(DWORD i=0;i<out->GetBufferSize();i++) text[i+4]=((char*)out->GetBufferPointer())[i];
		out->Release();
	}
	return text;
}

char* _stdcall Compile(char* in,int len,char* EntryPoint,char* Profile,byte Debug) {
	if(!d3dx) Load();
	ID3DXBuffer* out;
	ID3DXBuffer* errors;
	DWORD flags=0;
	if(Debug) flags=D3DXSHADER_DEBUG;
	if(FAILED(CompileShader(in,len,0,0,EntryPoint,Profile,flags,&out,&errors,0))) {
		if(out) out->Release();
		if(errors) {
			((DWORD*)text)[0]=0;
			for(DWORD i=0;i<errors->GetBufferSize();i++) text[i+4]=((char*)errors->GetBufferPointer())[i];
			errors->Release();
		} else {
			((DWORD*)text)[0]=0;
			((DWORD*)text)[1]=0;
		}
	} else {
		if(errors) errors->Release();
		((DWORD*)text)[0]=out->GetBufferSize();
		for(DWORD i=0;i<out->GetBufferSize();i++) text[i+4]=((char*)out->GetBufferPointer())[i];
		out->Release();
	}
	return text;
}