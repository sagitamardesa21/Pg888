// Copyright (c) 2016 Lorenzo Rossoni

#include "pch.h"
#include "ByteStreamHandler.h"

using namespace Unigram::Native;

HRESULT ByteStreamHandler::SetProperties(ABI::Windows::Foundation::Collections::IPropertySet* pConfiguration)
{
	return S_OK;
}

HRESULT ByteStreamHandler::CancelObjectCreation(IUnknown* pIUnknownCancelCookie)
{
	return E_NOTIMPL;
}

HRESULT ByteStreamHandler::GetMaxNumberOfBytesRequiredForResolution(QWORD* pqwBytes)
{
	if (pqwBytes == nullptr)
		return E_POINTER;

	*pqwBytes = GetMaxNumberOfBytesRequiredForResolution();
	return S_OK;
}

HRESULT ByteStreamHandler::BeginCreateObject(IMFByteStream* pByteStream, LPCWSTR pwszURL, DWORD dwFlags, IPropertyStore* pProps,
	IUnknown** ppIUnknownCancelCookie, IMFAsyncCallback* pCallback, IUnknown* pUnkState)
{
	if (pByteStream == nullptr)
		return E_POINTER;

	if (pCallback == NULL)
		return E_POINTER;

	if ((dwFlags & MF_RESOLUTION_MEDIASOURCE) == 0)
		return E_INVALIDARG;

	if (ppIUnknownCancelCookie)
		*ppIUnknownCancelCookie = nullptr;

	HRESULT result;
	if (FAILED(result = ValidateURL(pwszURL)) && FAILED(result = ValidateByteStream(pByteStream)))
		return result;

	ComPtr<IMFMediaSource> mediaSource;
	ReturnIfFailed(result, CreateMediaSource(pByteStream, pProps, &mediaSource));

	ComPtr<IMFAsyncResult> asyncResult;
	ReturnIfFailed(result, MFCreateAsyncResult(mediaSource.Get(), pCallback, pUnkState, &asyncResult));

	return MFInvokeCallback(asyncResult.Get());
}

HRESULT ByteStreamHandler::EndCreateObject(IMFAsyncResult *pResult, MF_OBJECT_TYPE* pObjectType, IUnknown** ppObject)
{
	if (pResult == nullptr || pObjectType == nullptr || ppObject == nullptr)
		return E_POINTER;

	*pObjectType = MF_OBJECT_MEDIASOURCE;
	return pResult->GetObject(ppObject);
}

bool ByteStreamHandler::CheckExtension(LPCWSTR url, LPCWSTR extension)
{
	if (url == nullptr)
		return true;

	auto extesionLength = wcslen(extension);
	auto pathLength = wcslen(url);
	if (pathLength < extesionLength)
		return false;

	return _wcsnicmp(url + pathLength - extesionLength, extension, extesionLength) == 0;
}
