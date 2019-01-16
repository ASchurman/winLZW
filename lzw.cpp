/* 
 * File:   lzw.cpp
 * Author: Alexander Schurman (alexander.schurman@yale.edu)
 *
 * Created on September 20, 2012
 * 
 * Provides functions to encode or decode a file using the LZW compression
 * algorithm
 */

#include <stdio.h>
#include <stdlib.h>
#include "code.h"
#include "lzw.h"
#include "stringTable.h"
#include "stack.h"

#define NBITS_MAXBITS (5) // the number of bits used to represent MAXBITS
#define NBITS_WINDOW (24) // the number of bits used to represent WINDOW
#define NBITS_EFLAG (1) // the number of bits used to represent -e

/*******************************************************************************
********************************** Encode **************************************
*******************************************************************************/

/* checks to see if the number of bits per code needs to be increased, and if so
 * sends the GROW_NBITS_CODE and increments nbits */
void checkNbits(unsigned char* nbits, stringTable* table, FILE* outfile)
{
    if(table->highestCode > (1 << *nbits) - 1)
    {
        putBits(*nbits, GROW_NBITS_CODE, outfile);
        (*nbits)++;
    }
}

/* outputs the escape character followed by k and updates the string table and
 * nbits */
void escapeChar(stringTable* table,
                pruneInfo* pi,
                unsigned char k,
                unsigned char* nbits,
                FILE* outfile)
{
    putBits(*nbits, ESCAPE_CODE, outfile);
    putBits(8, k, outfile);

    unsigned int newCode;
    stringTableAdd(table, EMPTY_PREFIX, k, &newCode);
    pruneInfoSawCode(pi, newCode);
    
    checkNbits(nbits, table, outfile);
}

/* checks to see if table should be pruned, and if so, prints the PRUNE_CODE,
 * calls stringTablePrune, updates nbits, and returns the new stringTable. If
 * no pruning occurs, the original stringTable* is returned. */
stringTable* checkPrune(stringTable* table,
                        pruneInfo* pi,
                        unsigned long window,
                        unsigned int* oldPrefix,
                        unsigned char* nbits,
                        FILE* outfile)
{
    if(window > 0 && stringTableIsFull(table))
    {
        putBits(*nbits, PRUNE_CODE, outfile);
        
        table = stringTablePrune(table, pi, window, oldPrefix);
        *oldPrefix = EMPTY_PREFIX;
        
        // update *nbits
        for(*nbits = 2; (1 << *nbits) -1 < table->highestCode; (*nbits)++);
    }
    
    return table;
}

void encode(FILE* infile, FILE* outfile, unsigned int maxBits, unsigned int window, bool eFlag)
{
    stringTable* table = stringTableNew(maxBits, eFlag);
    pruneInfo* pi = pruneInfoNew(maxBits);
    
    // write maxBits, window, and eFlag to outfile
    putBits(NBITS_MAXBITS, maxBits, outfile);
    putBits(NBITS_WINDOW, window, outfile);
    eFlag ? putBits(NBITS_EFLAG, 1, outfile) : putBits(NBITS_EFLAG, 0, outfile);
    
    // the string table is populated with (c, k) pairs; c is the code for the
    // prefix of the entry, k is the char appended to the end of the prefix
    unsigned int c = EMPTY_PREFIX;
    int k;
    
    unsigned char nbits = (eFlag) ? 2 : 9; // number of bits sent per code
    
    while((k = fgetc(infile)) != EOF)
    {
        tableElt* elt = stringTableHashSearch(table, c, k);
        
        if(elt)
        {
            c = elt->code;
        }
        else if(c == EMPTY_PREFIX)
        {
            // we're escaping k, so leave the prefix empty
            escapeChar(table, pi, k, &nbits, outfile);
            
            table = checkPrune(table, pi, window, &c, &nbits, outfile);
        }
        else
        {   
            putBits(nbits, c, outfile);
            pruneInfoSawCode(pi, c);
            
            stringTableAdd(table, c, k, NULL);
            
            table = checkPrune(table, pi, window, &c, &nbits, outfile);
            
            checkNbits(&nbits, table, outfile);
            
            tableElt* kCode = stringTableHashSearch(table, EMPTY_PREFIX, k);
            if(kCode)
            {
                c = kCode->code;
            }
            else
            {
                escapeChar(table, pi, k, &nbits, outfile);
                c = EMPTY_PREFIX; // since we escaped k, we now have no prefix
                checkPrune(table, pi, window, &c, &nbits, outfile);
            }
        }
    }
    
    if(c != EMPTY_PREFIX) putBits(nbits, c, outfile);
    
    putBits(nbits, STOP_CODE, outfile);
    flushBits(outfile);
    stringTableDelete(table);
    pruneInfoDelete(pi);
}


/*******************************************************************************
********************************** Decode **************************************
*******************************************************************************/

bool decode(FILE* infile, FILE* outfile)
{
    // get the header info
    unsigned int maxBits = getBits(NBITS_MAXBITS, infile);
    unsigned int window = getBits(NBITS_WINDOW, infile);
    char eFlag = getBits(NBITS_EFLAG, infile);
    
    stringTable* table = (eFlag == 1) ? stringTableNew(maxBits, true) : stringTableNew(maxBits, false);
    pruneInfo* pi = pruneInfoNew(maxBits);
    
    unsigned int oldCode = EMPTY_PREFIX; // the previous code read from infile
    unsigned int newCode; // the code just read from infile
    unsigned int code; // initially equal to newCode, but then set to its
                       // prefixes to obtain the entire string of newCode
    
    unsigned char finalK = 0, k; // characters in newCode
    stack* kStack = stackNew(); // holds characters from newCode in order to
                                // put them in the correct order by reversal
    
    unsigned char nbits = (eFlag) ? 2 : 9; // number of bits per code
    
    while((newCode = code = getBits(nbits, infile)) != STOP_CODE)
    {
        switch(code)
        {
            case EOF:
            {
                // getting EOF before the STOP_CODE is an error
                stringTableDelete(table);
                stackDelete(kStack);
                pruneInfoDelete(pi);
                return false;
                break;
            }
            
            case GROW_NBITS_CODE:
            {
                nbits++;
                if(nbits > maxBits)
                {
                    stringTableDelete(table);
                    stackDelete(kStack);
                    pruneInfoDelete(pi);
                    return false;
                }
                break;
            }
            
            case PRUNE_CODE:
            {
                if(window == 0)
                {
                    stringTableDelete(table);
                    stackDelete(kStack);
                    pruneInfoDelete(pi);
                    return false;
                }
                else
                {
                    table = stringTablePrune(table, pi, window, &oldCode);
                    
                    oldCode = EMPTY_PREFIX;

                    // update nbits
                    for(nbits = 2;
                        (1 << nbits) -1 < table->highestCode;
                        nbits++);
                }
                break;
            }
            
            case ESCAPE_CODE:
            {
                if(eFlag == 0)
                {
                    stringTableDelete(table);
                    stackDelete(kStack);
                    pruneInfoDelete(pi);
                    return false;
                }
                
                unsigned char escapedChar = getBits(8, infile);
                fputc(escapedChar, outfile);
                
                if(oldCode != EMPTY_PREFIX)
                {
                    stringTableAdd(table, oldCode, escapedChar, NULL);
                }
                
                unsigned int tempCode;
                stringTableAdd(table, EMPTY_PREFIX, escapedChar, &tempCode);
                pruneInfoSawCode(pi, tempCode);
                
                oldCode = EMPTY_PREFIX; // reset prefix to EMPTY
                break;
            }
            
            default:
            {
                pruneInfoSawCode(pi, newCode);
                
                if(!stringTableCodeSearch(table, code))
                {
                    stackPush(kStack, finalK);
                    code = oldCode;
                }

                // push string for code onto kStack until we get to the code
                // with an empty prefix, which goes into finalK
                tableElt* elt = stringTableCodeSearch(table, code);
                while(elt && elt->prefix != EMPTY_PREFIX)
                {
                    stackPush(kStack, elt->k);
                    code = elt->prefix;
                    
                    elt = stringTableCodeSearch(table, code);
                }
                finalK = elt->k;

                // print the characters in correct order now that they've been
                // reversed by pushing them onto kStack
                fputc(finalK, outfile);
                while(stackPop(kStack, &k))
                {
                    fputc(k, outfile);
                }

                // add oldCode to the table, then update it to the current code
                if(oldCode != EMPTY_PREFIX)
                {
                    stringTableAdd(table, oldCode, finalK, NULL);
                }
                oldCode = newCode;
                break;
            }
        }
    }
    
    stringTableDelete(table);
    stackDelete(kStack);
    pruneInfoDelete(pi);
    return true;
}