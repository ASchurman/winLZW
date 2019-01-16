// Originally written by Stan Eisenstat (09/23/08) at Yale University.
// Edited by Alexander Schurman to read/write any FILE* rather than stdin and stdout.
//
// Implementation of putBits/getBits described in code.h

#include <stdio.h>
#include <stdlib.h>
#include "code.h"

// Information shared by putBits() and flushBits()
static int nExtra = 0;                  // #bits from previous byte(s)
static unsigned int extraBits = 0;      // Extra bits from previous byte(s)


// == PUTBITS MODULE =======================================================

// Write CODE (NBITS bits) to standard output
void putBits (int nBits, int code, FILE* outfile)
{
    unsigned int c;

    code &= (1 << nBits) - 1;                   // Clear high-order bits
    nExtra += nBits;                            // Add new bits to extraBits
    extraBits = (extraBits << nBits) | code;
    while (nExtra >= CHAR_BIT) {                // Output any whole chars
	nExtra -= CHAR_BIT;                     //  and save remaining bits
	c = extraBits >> nExtra;
	fputc (c, outfile);
	extraBits ^= c << nExtra;
    }
}

// Flush remaining bits to standard output
void flushBits (FILE* outfile)
{
    if (nExtra != 0)
	fputc (extraBits << (CHAR_BIT - nExtra), outfile);
}


// == GETBITS MODULE =======================================================

// Return next code (#bits = NBITS) from input stream or EOF on end-of-file
int getBits (int nBits, FILE* infile)
{
    int c;
    static int nExtra = 0;          // #bits from previous byte(s)
    static int unsigned extra = 0;  // Extra bits from previous byte(s)

    // Read enough new bytes to have at least nBits bits to extract code
    while (nExtra < nBits) {
	if ((c = fgetc(infile)) == EOF)
	    return EOF;                         // Return EOF on end-of-file
	nExtra += CHAR_BIT;
	extra = (extra << CHAR_BIT) | c;
    }
    nExtra -= nBits;                            // Return nBits bits
    c = extra >> nExtra;
    extra ^= c << nExtra;                       // Save remainder
    return c;
}
