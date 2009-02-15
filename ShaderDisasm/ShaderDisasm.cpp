#include <d3d9.h>
#include <d3dx9.h>

char text[0x10000];

char* _stdcall Disasm(byte b[],int len,byte col) {
	ID3DXBuffer* out;
	if(FAILED(D3DXDisassembleShader((DWORD*)b,col,0,&out))) return 0;
	for(DWORD i=0;i<out->GetBufferSize();i++) text[i]=((char*)out->GetBufferPointer())[i];
	out->Release();
	return text;
}

char* _stdcall Asm(char* in,int len) {
	ID3DXBuffer* out;
	ID3DXBuffer* errors;
	if(D3DXAssembleShader(in,len,0,0,0,&out,&errors)!=D3D_OK) {
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
	ID3DXBuffer* out;
	ID3DXBuffer* errors;
	DWORD flags=0;
	if(Debug) flags=D3DXSHADER_DEBUG;
	if(FAILED(D3DXCompileShader(in,len,0,0,EntryPoint,Profile,flags,&out,&errors,0))) {
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