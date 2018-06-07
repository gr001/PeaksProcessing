#pragma once

namespace Bridge
{
	public ref class SectionNet sealed
	{
	public:
		property double Xscale;
		property System::String^ Description;
		property cli::array<double>^ Data;
	};

	public ref class ChannelNet sealed
	{
	public:
		property System::String^ Name;
		property System::String^ Yunits;
		property cli::array<SectionNet^>^ Sections;
	};

	public ref class RecordingNet sealed
	{
	public:
		property double Xscale;
		property double SamplingFrequency;
		property System::String^ Xunits;
		property System::String^ Comment;
		property System::String^ FileDescription;
		property System::String^ FilePath;
		property cli::array<ChannelNet^>^ Channels;
	};
}