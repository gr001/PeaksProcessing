// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

/*! \file stfio.h
 *  \author Christoph Schmidt-Hieber
 *  \date 2011-09-25
 *  \brief header file for libstfio
 * 
 * 
 *  Header file for libstfio
 */

#ifndef _STFIO_H_
#define _STFIO_H_

#include <iostream>
#include <boost/function.hpp>
#include <vector>
#include <deque>
#include <map>
#include <string>
#include <cmath>

#ifdef _MSC_VER
#pragma warning( disable : 4251 )  // Disable warning messages
#pragma warning( disable : 4996 )  // Disable warning messages
#endif

//! Defines dll export or import functions for Windows
//#if defined(_WINDOWS) && !defined(__MINGW32__)
//    #ifdef STFIODLL
//        #define StfioDll __declspec( dllexport )
//    #else
//        #define StfioDll __declspec( dllimport )
//    #endif
//#else
    #define StfioDll
//#endif

typedef std::vector<double > Vector_double;
typedef std::vector<float > Vector_float;

#ifdef _MSC_VER
    #ifndef NAN
        static const unsigned long __nan[2] = {0xffffffff, 0x7fffffff};
        #define NAN (*(const float *) __nan)
    #endif
    #ifndef INFINITY
        #define INFINITY (DBL_MAX+DBL_MAX)
    #endif
    //StfioDll long int lround(double x);
    #define snprintf _snprintf
#endif

#include "./recording.h"
#include "./channel.h"
#include "./section.h"

/* class Recording; */
/* class Channel; */
/* class Section; */

//! The stfio namespace.
/*! All essential core functions and classes are in this namespace. 
 *  Its purpose is to reduce name mangling problems.
 */
namespace stfio {

/*! \addtogroup stfio
 *  @{
 */

    StfioDll Vector_double vec_scal_plus(const Vector_double& vec, double scalar);

    StfioDll Vector_double vec_scal_minus(const Vector_double& vec, double scalar);

    StfioDll Vector_double vec_scal_mul(const Vector_double& vec, double scalar);

    StfioDll Vector_double vec_scal_div(const Vector_double& vec, double scalar);

    StfioDll Vector_double vec_vec_plus(const Vector_double& vec1, const Vector_double& vec2);

    StfioDll Vector_double vec_vec_minus(const Vector_double& vec1, const Vector_double& vec2);

    StfioDll Vector_double vec_vec_mul(const Vector_double& vec1, const Vector_double& vec2);

    StfioDll Vector_double vec_vec_div(const Vector_double& vec1, const Vector_double& vec2);

//! ProgressInfo class
/*! Abstract class to be used as an interface for the file io read/write functions
 *  Can be a GUI Dialog or stdout messages
 */
 class StfioDll ProgressInfo {
 public:
     //! Constructor
     /*! \param title Dialog title
      *  \param message Message displayed
      *  \param maximum Maximum value for the progress meter
      *  \param verbose Whether or not to emit a lot of noise
      */
     ProgressInfo(const std::string& title, const std::string& message, int maximum, bool verbose) {};

     //! Updates the progress info
     /*! \param value New value of the progress meter
      *  \param newmsg New message for the info text
      *  \param skip This is set to true if the user has chosen to skip the operation
      *  \return True unless the operation was cancelled.
      */
     virtual bool Update(int value, const std::string& newmsg="", bool* skip=NULL) = 0;
 };

 
//! StdoutProgressInfo class
/*! Example of a ProgressInfo that prints to stdout
 */
class StfioDll StdoutProgressInfo : public stfio::ProgressInfo {
 public:
    StdoutProgressInfo(const std::string& title, const std::string& message, int maximum, bool verbose);
    bool Update(int value, const std::string& newmsg="", bool* skip=NULL);
 private:
    bool verbosity;
};

//! Text file import filter settings
struct txtImportSettings {
  txtImportSettings() : hLines(1),toSection(true),firstIsTime(true),ncolumns(2),
        sr(20),yUnits("mV"),yUnitsCh2("pA"),xUnits("ms") {}

    int hLines;            /*!< Number of header lines. */
    bool toSection;        /*!< Import columns into separate sections rather than separate channels. */
    bool firstIsTime;      /*!< First column contains time. */
    int ncolumns;          /*!< Number of columns. */
    double sr;             /*!< Sampling rate. */
    std::string yUnits;    /*!< y units string. */
    std::string yUnitsCh2; /*!< y units string of second channel. */
    std::string xUnits;    /*!< x units string. */
};

//! File types
enum filetype {
    atf,    /*!< Axon text file. */
    abf,    /*!< Axon binary file. */
    axg,    /*!< Axograph binary file. */
    ascii,  /*!< Generic text file. */
    cfs,    /*!< CED filing system. */
    igor,   /*!< Igor binary wave. */
    son,    /*!< CED Son files. */
    hdf5,   /*!< hdf5 files. */
    heka,   /*!< heka files. */
    biosig, /*!< biosig files. */
    tdms,   /*!< TDMS files. */
    intan,   /*!< Intan CLAMP files. */
    none    /*!< Undefined file type. */
};

  
#ifndef TEST_MINIMAL
//! Attempts to determine the filetype from the filter extension.
/*! \param ext The filter extension to be tested (in the form wxT("*.ext")).
 *  \return The corresponding file type.
 */
StfioDll stfio::filetype
findType(const std::string& ext);
#endif // TEST_MINIMAL

//! Returns file extension for a file type
/*! \param ftype File type
 *  \return File extension for given file type (in the form ".ext")
 */
StfioDll std::string
findExtension(stfio::filetype ftype);

//! Generic file import.
/*! \param fName The full path name of the file. 
 *  \param type The file type. 
 *  \param ReturnData Will contain the file data on return.
 *  \param txtImport The text import filter settings.
 *  \param ProgressInfo Progress indicator
 *  \return true if the file has successfully been read, false otherwise.
 */
StfioDll bool 
importFile(
        const std::string& fName,
        stfio::filetype type,
        Recording& ReturnData,
        const stfio::txtImportSettings& txtImport,
        stfio::ProgressInfo& progDlg
);

//! Generic file export.
/*! \param fName The full path name of the file. 
 *  \param type The file type. 
 *  \param Data Data to be written
 *  \param ProgressInfo Progress indicator
 *  \return true if the file has successfully been written, false otherwise.
 */
StfioDll bool
exportFile(const std::string& fName, stfio::filetype type, const Recording& Data,
           ProgressInfo& progDlg);

//! Produce new recording with concatenated sections
/*! \param src Source recording
 *  \param sections Indices of selected sections
 *  \param ProgressInfo Progress indicator
 *  \return New recording with concatenated selected sections
 */
StfioDll Recording
concatenate(const Recording& src, const std::vector<std::size_t>& sections,
            ProgressInfo& progDlg);

//! Produce new recording with multiplied sections
/*! \param src Source recording
 *  \param sections Indices of selected sections
 *  \param channel Channel index
 *  \param factor Multiplication factor
 *  \return New recording with multiplied selected sections
 */
StfioDll Recording
multiply(const Recording& src, const std::vector<std::size_t>& sections,
         std::size_t channel, double factor);
/*@}*/

} // end of namespace

typedef std::vector< std::string >::iterator       sst_it;      /*!< std::string iterator */
typedef std::vector< std::string >::const_iterator c_sst_it;    /*!< constant std::string iterator */
typedef std::vector< std::size_t >::const_iterator c_st_it;     /*!< constant size_t iterator */
typedef std::vector< int >::iterator       int_it;      /*!< int iterator */
typedef std::vector< int >::const_iterator c_int_it;    /*!< constant int iterator */
typedef std::deque< Channel >::iterator       ch_it;       /*!< Channel iterator */
typedef std::deque< Channel >::const_iterator c_ch_it;     /*!< constant Channel iterator */
typedef std::deque< Section >::iterator       sec_it;      /*!< Section iterator */
typedef std::deque< Section >::const_iterator c_sec_it;    /*!< constant Section iterator */

#endif
