#include "pch.h"
#include "SignalingDataEmittedEventArgs.h"

namespace winrt::Unigram::Native::Calls::implementation
{
	SignalingDataEmittedEventArgs::SignalingDataEmittedEventArgs(IVector<uint8_t> data)
		: m_data(data)
	{
	}

	IVector<uint8_t> SignalingDataEmittedEventArgs::Data() {
		return m_data;
	}
} // namespace winrt::Unigram::Native::Calls::implementation
