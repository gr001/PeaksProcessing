#include "stdafx.h"
#include "DataReader.h"
#include "stfio.h"
#include <vcclr.h>  
#include "intan\common.h"

using namespace Bridge;

DataReader::DataReader()
{
}

RecordingNet^ DataReader::ReadFile(System::String^ filepath)
{
	RecordingNet^ ret = nullptr;

	Recording returnData;
	stfio::txtImportSettings txtImport;
	stfio::StdoutProgressInfo progDlg("Title", "Message", 0, true);

	cli::pin_ptr<const wchar_t> pinned_ptr = PtrToStringChars(filepath);
	std::wstring wstrPath(static_cast<const wchar_t *>(pinned_ptr), filepath->Length);

	std::string strPath(toString(wstrPath));
	//filepath->

	if (stfio::importFile(strPath, stfio::filetype::abf, returnData, txtImport, progDlg))
	{
		ret = gcnew RecordingNet();

		ret->FilePath = filepath;
		ret->SamplingFrequency = returnData.GetSR();
		ret->Xunits = gcnew System::String(returnData.GetXUnits().c_str(), 0, (int)returnData.GetXUnits().length());
		ret->Xscale = returnData.GetXScale();
		ret->FileDescription = gcnew System::String(returnData.GetFileDescription().c_str(), 0, (int)returnData.GetFileDescription().length());
		ret->Comment = gcnew System::String(returnData.GetComment().c_str(), 0, (int)returnData.GetComment().length());

		const int channelsCount = (int)returnData.size();
		if (channelsCount)
		{
			ret->Channels = gcnew cli::array<ChannelNet^>(channelsCount);

			for (int iChannel = 0; iChannel < channelsCount; iChannel++)
			{
				const auto& channel = returnData[iChannel];
				ChannelNet^ channelNet = ret->Channels[iChannel] = gcnew ChannelNet();
				
				channelNet->Name = gcnew System::String(channel.GetChannelName().c_str(), 0, (int)channel.GetChannelName().length());
				channelNet->Yunits = gcnew System::String(channel.GetYUnits().c_str(), 0, (int)channel.GetYUnits().length());

				const int sectionsCount = (int)channel.size();

				if (sectionsCount)
				{
					channelNet->Sections = gcnew cli::array<SectionNet^>(sectionsCount);

					for (int iSection = 0; iSection < sectionsCount; iSection++)
					{
						const auto& section = channel[iSection];
						SectionNet^ sectionNet = channelNet->Sections[iSection] = gcnew SectionNet();
						sectionNet->Xscale = section.GetXScale();
						sectionNet->Description = gcnew System::String(section.GetSectionDescription().c_str(), 0, (int)section.GetSectionDescription().length());

						const int dataCount = (int)section.size();
						
						if (dataCount)
						{
							sectionNet->Data = gcnew cli::array<double>(dataCount);
							double d;

							for (int idata = 0; idata < dataCount; idata++)
							{
								d = sectionNet->Data[idata] = section[idata];
							}
						}
					}
				}
			}
		}
	}

	return ret;
}