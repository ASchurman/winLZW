/* Alexander Schurman
 * 
 * Created on October 24, 2012
 *
 * A managed wrapper for lzw */

using namespace System::Runtime::InteropServices;

#include <stdio.h>
#include "lzw.h"
#include "ManagedLZW.h"

void openFiles(System::String^ filenameIn, System::String^ filenameOut, FILE** infile, FILE** outfile)
{
    System::IntPtr instrPtr = Marshal::StringToHGlobalAnsi(filenameIn);
    System::IntPtr outstrPtr = Marshal::StringToHGlobalAnsi(filenameOut);

    char* instr = (char*)(void*)instrPtr;
    char* outstr = (char*)(void*)outstrPtr;

    *infile = fopen(instr, "rb");
    if(!(*infile))
    {
        Marshal::FreeHGlobal(instrPtr);
        Marshal::FreeHGlobal(outstrPtr);
        throw gcnew System::ArgumentException("Could not open input file", "filenameIn");
    }

    *outfile = fopen(outstr, "wb+");
    if(!(*outfile))
    {
        fclose(*infile);
        Marshal::FreeHGlobal(instrPtr);
        Marshal::FreeHGlobal(outstrPtr);
        throw gcnew System::ArgumentException("Could not open output file", "filenameOut");
    }

    Marshal::FreeHGlobal(instrPtr);
    Marshal::FreeHGlobal(outstrPtr);
}

void ManagedLZW::ManagedEncode(System::String^ filenameIn, System::String^ filenameOut, unsigned int maxBits, unsigned int window, bool eFlag)
{
    FILE* infile;
    FILE* outfile;
    openFiles(filenameIn, filenameOut, &infile, &outfile);

    encode(infile, outfile, maxBits, window, eFlag);

    fclose(infile);
    fclose(outfile);
}

void ManagedLZW::ManagedDecode(System::String^ filenameIn, System::String^ filenameOut)
{
    FILE* infile;
    FILE* outfile;
    openFiles(filenameIn, filenameOut, &infile, &outfile);

    bool decodeSuccess = decode(infile, outfile);

    fclose(infile);
    fclose(outfile);

    if(!decodeSuccess)
    {
        throw gcnew System::ArgumentException("Not a valid LZW-encoded file", "filenameIn");
    }
}