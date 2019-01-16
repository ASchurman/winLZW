// Originally written by Stan Eisenstat (09/23/08) at Yale University.
// Edited by Alexander Schurman to read/write any FILE* rather than stdin and stdout.
//
// Interface to putBits/getBits

#include <limits.h>

// Write code (#bits = nBits) to outfile.
// [Since bits are written as CHAR_BIT-bit characters, any extra bits are
//  saved, so that final call must be followed by call to flushBits().]
void putBits (int nBits, int code, FILE* outfile);

// Flush any extra bits to outfile
void flushBits (FILE* outfile);

// Return next code (#bits = nBits) from infile (EOF on end-of-file)
int getBits (int nBits, FILE* infile);
