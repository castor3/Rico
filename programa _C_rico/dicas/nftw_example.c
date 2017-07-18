// EXAMPLE
//        The following program traverses the directory tree under the path
//	      named in its first command-line argument, or under the current
//        directory if no argument is supplied.  It displays various
//        information about each file.  The second command-line argument can be
//        used to specify characters that control the value assigned to the
//        flags argument when calling nftw().

//		Program source
#define _XOPEN_SOURCE 500
#include <ftw.h>
#include <stdio.h>
#include <stdlib.h>	//for exit(); / EXIT_FAILURE / ...
#include <string.h>	//for strchr();
#include <unistd.h>	//for sleep(1);
/*
static int display_info(const char *fpath, const struct stat *sb,
						int tflag, struct FTW *ftwbuf){
	printf("%-3s %2d %7jd   %-40s %d %s\n",
	   (tflag == FTW_D) ?   "d"   : (tflag == FTW_DNR) ? "dnr" :
	   (tflag == FTW_DP) ?  "dp"  : (tflag == FTW_F) ?   "f" :
	   (tflag == FTW_NS) ?  "ns"  : (tflag == FTW_SL) ?  "sl" :
	   (tflag == FTW_SLN) ? "sln" : "???",
	   ftwbuf->level, (intmax_t) sb->st_size,
	   fpath, ftwbuf->base, fpath + ftwbuf->base);
	return 0;       // To tell nftw() to continue
}
*/

static int display_info(const char *fpath, const struct stat *sb,
						int tflag, struct FTW *ftwbuf){

/*	printf("vai adicionar novo caminho\n");
	sleep(1);
	printf("Este e o caminho:%s\n", fpath);
	sleep(1);*/
	printf("%5d  %-45s %s\n",
	   ftwbuf->level, fpath, fpath + ftwbuf->base);
	return 0;           /* To tell nftw() to continue */
}

int main(int argc, char *argv[]){

	int flags = 0;

	if (argc > 2 && strchr(argv[2], 'd') != NULL)
		flags |= FTW_DEPTH;
	if (argc > 2 && strchr(argv[2], 'p') != NULL)
		flags |= FTW_PHYS;

	printf("%s  %s", "Level", "Path\n");
	if (nftw((argc < 2) ? "." : argv[1], display_info, 1, flags) == -1){
		perror("nftw");
   		exit(EXIT_FAILURE);
	}
	exit(EXIT_SUCCESS);
}


