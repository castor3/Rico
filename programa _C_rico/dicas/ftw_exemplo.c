#include <ftw.h>
#include <stdio.h>
#include <string.h>

int list(const char *name, const struct stat *status, int type/*, FILE *paths*/);

int main(int argc, char *argv[]) {

// 	FILE *paths = fopen("caminhos_ficheiros.txt", "a+");

	if(argc == 1)
		ftw(".", list, 1);
	else
		ftw(argv[1], list, 1);
	return 0;
}

// FTW_F    The object is a  file
// FTW_D    ,,    ,,   ,, ,, directory
// FTW_DNR  ,,    ,,   ,, ,, directory that could not be read
// FTW_SL   ,,    ,,   ,, ,, symbolic link
// FTW_NS   The object is NOT a symbolic link and is one for
//          which stat() could not be executed
int list(const char *name, const struct stat *stat, int type/*, FILE *paths*/) {
	if(type == FTW_NS)
		return 0;

	if(type == FTW_F){
		if(!strcmp(name, "machineparameters.txt")){
			printf("tamanho:%u", stat->st_rdev);
		}
		printf("0%3o\t%s\n", status->st_mode&0777, name);
	}

	if(type == FTW_D && strcmp(".", name) != 0)
		printf("0%3o\t%s/\n", status->st_mode&0777, name);

	return 0;
}