/* 
 * File:   lzw.h
 * Author: Alexander Schurman
 *
 * Created on September 20, 2012
 * 
 * Provides functions to encode and decode a file with the LZW compression
 * algorithm
 */

#ifndef LZW_H
#define LZW_H

/* encodes infile into outfile.
 * maxBits is the maximum number of bits allowed per code. It must be in the
 *     range [8, 24]
 * window is the window size for pruning. If zero, no pruning will happen.
 * eFlag indicates if encode was passed the -e argument. */
void encode(FILE* infile, FILE* outfile, unsigned int maxBits, unsigned int window, bool eFlag);

/* decodes infile into outfile. Returns true if successful, false if infile is an
 * invalid encoded stream */
bool decode(FILE* infile, FILE* outfile);

#endif
