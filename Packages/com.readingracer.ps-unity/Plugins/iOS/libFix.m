#include <stdio.h>
#include <sys/stat.h>
#include <sys/mman.h>

FILE *fopen$UNIX2003( const char *filename, const char *mode )
{
    return fopen(filename, mode);
}

int open$UNIX2003(char* filename, int access, int permission)
{
    return open(filename, access, permission);
}

size_t fwrite$UNIX2003( const void *a, size_t b, size_t c, FILE *d )
{
    return fwrite(a, b, c, d);
}

int fstat$INODE64(int fildes, struct stat* buf)
{
    return fstat(fildes,buf);
}

void* mmap$UNIX2003(void* address, size_t length, int protect, int flags, int fildes, off_t offset)
{
    return mmap(address,length,protect,flags,fildes,offset);
}

int close$UNIX2003(int handle)
{
    return close(handle);
}