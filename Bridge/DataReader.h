#pragma once

#include "RecordingNet.h"

namespace Bridge
{
	public ref class DataReader
	{
	public:
		DataReader();

		RecordingNet^ ReadFile(System::String^ filepath);
	};
}