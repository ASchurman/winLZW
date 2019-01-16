/* Alexander Schurman
 * 
 * Created on October 24, 2012
 *
 * A managed wrapper for lzw */

#ifndef MANAGEDLZW_H
#define MANAGEDLZW_H

public ref class ManagedLZW
{
public:
    ManagedLZW() { }
    ~ManagedLZW() { }

    /* encodes the file with name filenameIn and outputs the encoded file as filenameOut.
     * maxBits is the maximum number of bits per code
     * window is the size of the pruning window (0 if no pruning is to occur)
     * eFlag == true if the string table isn't to be initialized with the single-char strings */
    void ManagedEncode(System::String^ filenameIn, System::String^ filenameOut, unsigned int maxBits, unsigned int window, bool eFlag);

    /* decodes the file with name filenameIn and outputs the decoded file as filenameOut */
    void ManagedDecode(System::String^ filenameIn, System::String^ filenameOut);
};

#endif